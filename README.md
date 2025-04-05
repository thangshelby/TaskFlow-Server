# TaskFlow - Project Management System Using DI & Clean Architechture🚀  

**TaskFlow** is a project management system inspired by **Jira**, built using a **microservices architecture**. It is designed for scalability and flexibility, leveraging the best technologies suited for each service's requirements.  

---

## ⚙️ Technologies Used  

### Backend Services:  
- **C#**  
  - ASP.NET Core for `main-service` responsible for managing tasks, users, groups, projects and boards  
- **Go**  
  - [Gin](https://github.com/gin-gonic/gin) for `metrics-service` to collect and expose metrics  
- **Node.js + TypeScript**  
  - [Express](https://expressjs.com/) for `notification-service` and `payment-service`  

### Infrastructure & Communication:  
- **RabbitMQ** – For asynchronous messaging between services  
- **gRPC** – For high-performance synchronous communication  
- **Envoy** – API Gateway for routing and managing microservices  
- **Docker** – Containerized services  
- **Kubernetes (Optional)** – Deployment orchestration  
- **MySQL/PostgreSQL/MongoDB** – Database (depending on service needs)  
- **Redis** – Caching layer  
- **Clean Architecture** – For maintainable and testable codebase  

---

## 🚀 Getting Started  

### Prerequisites  

Ensure you have the following installed:  
- [Docker](https://www.docker.com/)  
- [Node.js](https://nodejs.org/) (for Node services)  
- [Go](https://go.dev/) (for Go services)  
- [RabbitMQ](https://www.rabbitmq.com/) (can be run via Docker)  
- [ASP.NET Core](https://dotnet.microsoft.com/) (for main service)  
- [Envoy](https://www.envoyproxy.io/docs/envoy/latest/#) (API Gateway)  

---

## 🔥 To-Do Features:  

1. **Implement gRPC** for High-Performance Interactions  
2. **Add Authentication using JWT**  
3. **Improve Logging** with a Centralized System  
5. **Integrate Real-Time Notifications**  
6. **Enhance Metrics and Monitoring Dashboard**  

---

Feel free to contribute and modify this template to suit your needs! 🚀
