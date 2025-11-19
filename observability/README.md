# Tempus Observability Stack

This directory contains the configuration for the Tempus observability stack using OpenTelemetry, Prometheus, Jaeger, and Grafana.

## Components

### 1. **Jaeger** (Distributed Tracing)
- **URL**: http://localhost:16686
- **Purpose**: Visualize distributed traces and track request flows through the application
- **OTLP Endpoint**: localhost:4317 (gRPC), localhost:4318 (HTTP)

### 2. **Prometheus** (Metrics Collection)
- **URL**: http://localhost:9090
- **Purpose**: Collect and store time-series metrics from the application
- **Scrapes**: Tempus app on port 5000 every 5 seconds

### 3. **Grafana** (Visualization & Dashboards)
- **URL**: http://localhost:3000
- **Default Credentials**: admin / admin
- **Purpose**: Create dashboards and visualize metrics and traces
- **Pre-configured Datasources**: Prometheus and Jaeger

### 4. **OpenTelemetry Collector** (Optional)
- **URL**: http://localhost:8888 (metrics)
- **Purpose**: Central telemetry data collection, processing, and routing
- **OTLP Endpoint**: localhost:4317 (gRPC), localhost:4318 (HTTP)

## Quick Start

### 1. Start the Observability Stack

```bash
# From the tempus root directory
docker-compose up -d

# Check that all containers are running
docker-compose ps

# View logs
docker-compose logs -f
```

### 2. Configure Your Application

Update your `appsettings.Development.json` or `Program.cs` to send telemetry data:

**For direct export to Jaeger:**
```json
{
  "OpenTelemetry": {
    "Jaeger": {
      "AgentHost": "localhost",
      "AgentPort": 6831,
      "Endpoint": "http://localhost:14268/api/traces"
    },
    "ServiceName": "tempus-web"
  }
}
```

**For OTLP export (recommended):**
```csharp
// In Program.cs, configure OTLP exporter
services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
        }))
    .WithMetrics(builder => builder
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
        }));
```

### 3. Run Your Application

```bash
cd src/Tempus.Web
dotnet run
```

### 4. Access the Dashboards

| Service | URL | Description |
|---------|-----|-------------|
| **Grafana** | http://localhost:3000 | Main dashboard (admin/admin) |
| **Jaeger UI** | http://localhost:16686 | View distributed traces |
| **Prometheus** | http://localhost:9090 | Query metrics directly |

## Viewing Telemetry Data

### Traces in Jaeger

1. Open http://localhost:16686
2. Select "tempus-web" from the Service dropdown
3. Click "Find Traces"
4. Click on any trace to see the detailed span timeline

### Metrics in Prometheus

1. Open http://localhost:9090
2. Go to Graph tab
3. Try queries like:
   - `http_server_request_duration_seconds_bucket` - HTTP request duration
   - `process_runtime_dotnet_gc_collections_count` - GC collections
   - `process_cpu_seconds_total` - CPU usage

### Creating Grafana Dashboards

1. Open http://localhost:3000 and login (admin/admin)
2. Click "+" → "Import Dashboard"
3. Import these popular dashboards:
   - **ASP.NET Core**: Dashboard ID 10915
   - **.NET Runtime Metrics**: Dashboard ID 12489
   - **HTTP Request Overview**: Dashboard ID 12230

Or create custom dashboards:
1. Click "+" → "Dashboard"
2. Add Panel
3. Select Prometheus as data source
4. Write PromQL queries

## Useful Prometheus Queries

```promql
# Request rate per second
rate(http_server_request_duration_seconds_count[5m])

# 95th percentile response time
histogram_quantile(0.95, rate(http_server_request_duration_seconds_bucket[5m]))

# Memory usage
process_working_set_bytes / 1024 / 1024

# GC pause time
rate(process_runtime_dotnet_gc_pause_duration_seconds_sum[5m])

# Active HTTP requests
http_server_active_requests
```

## Troubleshooting

### No data in Jaeger/Prometheus

1. **Check Tempus app is exporting telemetry:**
   ```bash
   # Check app logs for OpenTelemetry initialization
   dotnet run | grep -i "opentelemetry"
   ```

2. **Check Prometheus targets:**
   - Go to http://localhost:9090/targets
   - Verify "tempus-web" target is UP
   - If DOWN, check the target address (host.docker.internal:5000)

3. **Check container logs:**
   ```bash
   docker-compose logs jaeger
   docker-compose logs prometheus
   docker-compose logs otel-collector
   ```

### Port conflicts

If ports are already in use, edit `docker-compose.yml`:
```yaml
ports:
  - "3001:3000"  # Change Grafana to port 3001
```

### Can't access application from Prometheus

On **macOS/Windows Docker Desktop**:
- Use `host.docker.internal` instead of `localhost`
- Already configured in `prometheus.yml`

On **Linux**:
- Change `host.docker.internal:5000` to `172.17.0.1:5000` in `prometheus.yml`
- Or use `--network host` mode

## Stopping the Stack

```bash
# Stop all containers
docker-compose down

# Stop and remove volumes (deletes all data)
docker-compose down -v
```

## Advanced Configuration

### Custom Grafana Dashboards

Place JSON dashboard files in:
```
observability/grafana/provisioning/dashboards/
```

### Prometheus Alert Rules

Create `observability/prometheus-alerts.yml`:
```yaml
groups:
  - name: tempus_alerts
    rules:
      - alert: HighErrorRate
        expr: rate(http_server_request_duration_seconds_count{status=~"5.."}[5m]) > 0.05
        for: 5m
        annotations:
          summary: "High error rate detected"
```

### Add Loki for Logs (Optional)

Add to `docker-compose.yml`:
```yaml
loki:
  image: grafana/loki:latest
  ports:
    - "3100:3100"
  volumes:
    - ./observability/loki-config.yml:/etc/loki/local-config.yaml
```

## Resources

- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/instrumentation/net/)
- [Prometheus Query Examples](https://prometheus.io/docs/prometheus/latest/querying/examples/)
- [Jaeger Documentation](https://www.jaegertracing.io/docs/)
- [Grafana Dashboard Gallery](https://grafana.com/grafana/dashboards/)
