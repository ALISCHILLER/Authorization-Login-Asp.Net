# ASP.NET Core Authentication & Authorization System

A modern, secure, and scalable authentication and authorization system built with ASP.NET Core, following Clean Architecture principles.

## Features

- 🔐 **Advanced Authentication**
  - JWT-based authentication with refresh tokens
  - Two-factor authentication (2FA) with TOTP
  - Password policies with strong requirements
  - Account lockout and rate limiting
  - Email and SMS verification

- 🛡️ **Security**
  - Content Security Policy (CSP)
  - HTTPS enforcement
  - Rate limiting
  - IP-based restrictions
  - Secure password hashing
  - Protection against common attacks

- 📊 **Monitoring & Observability**
  - Health checks for all services
  - Prometheus metrics
  - Distributed tracing with OpenTelemetry
  - Structured logging with Serilog
  - Performance monitoring

- 🚀 **Performance**
  - Redis caching
  - Response compression
  - Output caching
  - Connection pooling
  - Async operations

## Architecture

The project follows Clean Architecture principles with the following layers:

- **Domain**: Core business entities and interfaces
- **Application**: Business logic and use cases
- **Infrastructure**: External services, data access, and security
- **API**: Controllers and endpoints

## Prerequisites

- .NET 8.0 SDK
- SQL Server or PostgreSQL
- Redis
- Seq (for logging)
- Prometheus (for metrics)
- Jaeger (for tracing)

## Getting Started

1. Clone the repository:
```bash
git clone https://github.com/yourusername/auth-system.git
cd auth-system
```

2. Configure environment variables:
```bash
# Development
dotnet user-secrets init
dotnet user-secrets set "JWT:SecretKey" "your-secret-key"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
dotnet user-secrets set "Redis:ConnectionString" "your-redis-connection"
```

3. Update appsettings.json with your configuration:
```json
{
  "AppSettings": {
    "JwtSettings": {
      "Issuer": "your-issuer",
      "Audience": "your-audience"
    }
  }
}
```

4. Run the application:
```bash
dotnet run
```

## API Documentation

The API documentation is available at `/swagger` when running in development mode.

### Key Endpoints

- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login with credentials
- `POST /api/auth/refresh-token` - Refresh JWT token
- `POST /api/auth/verify-2fa` - Verify 2FA code
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password

## Security

### Password Requirements

- Minimum length: 12 characters
- Must contain uppercase and lowercase letters
- Must contain numbers
- Must contain special characters
- Maximum repeating characters: 2
- Password history: 5 previous passwords

### Rate Limiting

- Global: 100 requests per minute
- Login attempts: 5 per 15 minutes
- Password reset: 3 per hour
- 2FA verification: 3 per 5 minutes

## Monitoring

### Health Checks

- Database connectivity
- Redis connection
- External API dependencies
- Custom health checks

Access health check UI at `/health-ui`

### Metrics

Prometheus metrics are available at `/metrics`:

- Request duration
- Error rates
- Active users
- Cache hit/miss ratios
- Database query performance

### Logging

Logs are sent to multiple sinks:

- Console (development)
- File (JSON format)
- Seq (structured logging)
- Elasticsearch (production)

## Testing

Run the test suite:

```bash
dotnet test
```

Generate coverage report:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## Deployment

### Docker

Build the image:

```bash
docker build -t auth-system .
```

Run the container:

```bash
docker run -d -p 5000:80 -p 5001:443 \
  -e ConnectionStrings__DefaultConnection="your-connection-string" \
  -e JWT__SecretKey="your-secret-key" \
  auth-system
```

### Kubernetes

Deploy to Kubernetes:

```bash
kubectl apply -f k8s/
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [JWT](https://jwt.io/)
- [Serilog](https://serilog.net/)
- [Prometheus](https://prometheus.io/)
- [OpenTelemetry](https://opentelemetry.io/) 