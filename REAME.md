Что делает этот проект?
Реализованы два сервиса, которые обмениваются информацией при помощи кафки
1. Сервис монет
2. Севис пользователей
БД: mongodb
Кэширование: redis
Замеры показателей: Prometheus, Grafana

Чтобы запустить все сервисы (Linux)
Запустить в терминале: docker-compose up -d
На Windows нужно ещё предварительно запустить приложеине docker desktop

Далее, сервисы будут доступны по адресам:
- Swagger монет: http://localhost:5174/swagger

- Swagger пользователей: http://localhost:5180/swagger

- Prometheus: http://localhost:9090

- Grafana: http://localhost:3000 (admin/admin)