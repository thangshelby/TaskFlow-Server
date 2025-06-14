# TaskFlow - Project Management System Using DI & Clean Architechture🚀

**TaskFlow** is a project management system inspired by **Jira**, built using a **microservices architecture**. It is designed for scalability and flexibility, leveraging the best technologies suited for each service's requirements.

---

### 📦 Service Port Mapping

| Service Name           | HTTP Port         | gRPC Port        |
| ---------------------- | ----------------- | ---------------- |
| `main-service`         | `localhost:8081`  | `localhost:5001` |
| `notification-service` | `localhost:8082`  | `localhost:5002` |
| `chat-service`         | `none`            | `none`           |
| `metrics-service`      | `none`            | `none`           |
| `envoy`                | `localhost:9901`  | `none`           |
| `kafka`                | `localhost:9092`  | `none`           |
| `mysql`                | `localhost:3306`  | `none`           |
| `mongodb`              | `localhost:27017` | `none`           |
| `kafka_ui`             | `localhost:8888`  | `none`           |
| `cassandra`            | `localhost:9042`  | `none`           |
| `redis`                | `localhost:6379`  | `none`           |

---

## ⚙️ Technologies Used

### Backend Services:

- **C#**
  - ASP.NET Core for `main-service` responsible for managing tasks, users, groups, projects and boards
- **Go**
  - [Gin](https://github.com/gin-gonic/gin) for `metrics-service` to collect and expose metrics
- **Node.js + TypeScript**
  - [NestJS](https://docs.nestjs.com/) for `notification-service` and `chat-service`

### Infrastructure & Communication:

- **Apache Kafka** – For asynchronous messaging between services
- **gRPC** – For high-performance synchronous communication
- **Envoy** – API Gateway for routing and managing microservices
- **Docker** – Containerized services
- **Kubernetes (Optional)** – Deployment orchestration
- **MySQL/PostgreSQL/MongoDB/Cassandra** – Database (depending on service needs)
- **Redis** – Caching layer
- **Clean Architecture** – For maintainable and testable codebase

---

## 🚀 Getting Started

### Prerequisites

Ensure you have the following installed:

- [Docker](https://www.docker.com/)
- [Node.js](https://nodejs.org/) (for Nest services, v22.x)
- [Go](https://go.dev/) (for Go services)
- [Kafka](https://kafka.apache.org/) (can be run via Docker)
- [ASP.NET Core](https://dotnet.microsoft.com/) (for main service)
- [Envoy](https://www.envoyproxy.io/docs/envoy/latest/#) (API Gateway)

---

Feel free to contribute and modify this template to suit your needs! 🚀
