Kubernetes и мониторинг — памятка
=================================

Что добавлено
-------------
- k8s/dependencies.yaml — MongoDB и Redis (Deployment + Service), по 1 реплике.
- k8s/storeofcoinsapi.yaml — Deployment 3 реплики образа storeofcoinsapi:local, Service NodePort (30080->5174), пробы /metrics, HPA CPU 70% (min 3, max 6).
- k8s/usersapi.yaml — Deployment 3 реплики образа usersapi:local, Service NodePort (30081->5180), пробы /metrics, HPA CPU 70% (min 3, max 6).
- k8s/servicemonitors.yaml — ServiceMonitor для Prometheus Operator, чтобы скрейпить /metrics у обоих сервисов.
- metrics-server включен для HPA: `minikube addons enable metrics-server`.
- kube-prometheus-stack (Prometheus + Grafana + kube-state-metrics + node-exporter) установлен в namespace monitoring через Helm.

Как развернуть (кластер уже запущен)
------------------------------------
```powershell
kubectl apply -f k8s/dependencies.yaml
kubectl apply -f k8s/storeofcoinsapi.yaml
kubectl apply -f k8s/usersapi.yaml
kubectl apply -f k8s/servicemonitors.yaml
kubectl get pods -o wide
```
Все поды должны быть Ready.

Доступ к сервисам
-----------------
- StoreOfCoinsApi: `kubectl port-forward svc/storeofcoinsapi 8080:80` → http://localhost:8080/swagger и http://localhost:8080/graphql
- UsersApi: `kubectl port-forward svc/usersapi 8081:80` → http://localhost:8081/swagger

Мониторинг: Prometheus и Grafana
--------------------------------
- Grafana: `kubectl -n monitoring port-forward svc/kps-grafana 3000:80`
- Пароль admin:
  ```powershell
  kubectl -n monitoring get secret kps-grafana -o jsonpath="{.data.admin-password}" |
    % { [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($_)) }
  ```
- Проверка таргетов Prometheus:  
  `kubectl -n monitoring port-forward svc/kps-kube-prometheus-stack-prometheus 9090:9090` → http://localhost:9090 → Status → Targets. Таргеты ServiceMonitor для storeofcoinsapi и usersapi должны быть UP.
- В Grafana Prometheus datasource уже настроен. В Explore/панелях используйте запросы:
  - Пользовательские: `coins_read`, `coins_write`, `coins_created`, `coins_updated`, `coins_deleted`, `mongo_request_duration_ms` (появятся после вызовов API/GraphQL).
  - Общие ASP.NET/OTEL: `http_server_duration_seconds_*`, `process_cpu_seconds_total`, `process_working_set_bytes`, `dotnet_total_memory_bytes`, `dotnet_gc_*`.
- Если метрик не видно — сгенерируйте трафик (например, POST `/api/Test/add-100-coins`) и подождите несколько секунд.

Что значат метрики и как влиять
-------------------------------
- `coins_*` — счётчики операций с БД; растут при запросах.
- `mongo_request_duration_ms` — длительность операций MongoDB.
- `http_server_duration_seconds_*` — латентность HTTP.
- `process_cpu_seconds_total`, `process_working_set_bytes`, `dotnet_total_memory_bytes`, `dotnet_gc_*` — CPU, память, GC .NET.
Влияние: нагрузка (больше запросов), requests/limits в Deployment, масштабирование через HPA.

HPA (Horizontal Pod Autoscaler)
-------------------------------
- Описан в storeofcoinsapi.yaml и usersapi.yaml (autoscaling/v2): minReplicas=3, maxReplicas=6, target CPU 70%.
- Источник метрик: metrics-server.
- Логика: при средней CPU >70% — увеличивает реплики (до 6), при низкой — уменьшает, но не ниже 3.

Тест отказоустойчивости (п.5)
----------------------------
Цель: при уходе worker-ноды поды пересоздаются на оставшихся.
```powershell
kubectl get nodes
kubectl drain <worker-node> --ignore-daemonsets --delete-emptydir-data
kubectl delete node <worker-node>
kubectl get pods -o wide --watch
```
ReplicaSet/Deployment поднимет недостающие поды на живых нодах. Вернуть воркер: `minikube node add` (нужен доступ к registry.k8s.io для kicbase).

Сборка и загрузка образов (при изменении кода)
----------------------------------------------
```powershell
docker build -t storeofcoinsapi:local StoreOfCoinsApi
docker build -t usersapi:local UsersApi
minikube image load storeofcoinsapi:local
minikube image load usersapi:local
```

ServiceMonitor — как работает
-----------------------------
`k8s/servicemonitors.yaml` создаёт ServiceMonitor с меткой `release: kps`. Prometheus из kube-prometheus-stack выбирает сервисы `storeofcoinsapi` и `usersapi` (порт `http`, путь `/metrics`) и опрашивает их. Поэтому в Grafana видны метрики приложений, а не только кластера.
