# Real-Time Change Data Capture (CDC) Pipeline

A robust, event-driven microservices architecture demonstrating real-time data migration and consumption. This pipeline captures database changes from MongoDB, streams them through Kafka, and routes the events to both a live internal monitoring dashboard (via SignalR) and an external Gateway API for downstream consumers.

## System Architecture

1. **Source Database (MongoDB Atlas):** The origin of data operations, subjected to live traffic via a Go workload generator.
2. **Event Queue (Confluent Kafka):** Acts as a decoupled shock absorber, queuing real-time database changes into the `cdc_benchmark` topic.
3. **Change Stream Manager (C#):** A microservice that acts as the Kafka consumer, instantly pulling events and publishing them to an internal asynchronous Message Bus.
4. **CDC Target / Gateway (C#):** A Web API that ingests the Message Bus payload, persists audit logs in a local SQLite database, and acts as the secure gateway for external systems.
5. **Internal Migration Dashboard:** A lightweight HTML interface injected directly into the CDCTarget service, utilizing SignalR (WebSockets) to visualize pipeline throughput and data payloads the millisecond they cross the internal network.
6. **Downstream Consumer (Swagger UI):** Represents a third-party client subscribing to the pipeline and querying perfectly formatted JSON payloads.

---

## Prerequisites

To run this project locally, ensure you have the following installed and configured:
* **.NET 8.0 SDK** (or matching framework version)
* **Go** (for the workload generator script)
* **MongoDB Atlas** (Active cluster with connection string)
* **Confluent Kafka** (Active cluster with topics configured)
* **Visual Studio** (or preferred C# IDE)

---

## Execution Steps (Demo Flow)

Follow these steps to run the complete end-to-end pipeline demonstration:

### 1. Start the Live Workload
Open a terminal, navigate to your Go script directory, and start pumping traffic into the source database:
```bash
go run workload_generator.go
```
*(Leave this running in the background to simulate a live production environment).*

### 2. Start the Core Microservices
Open the C# Solution in Visual Studio.
Ensure your solution is configured to start multiple projects:
1. Right-click the Solution node -> **Configure Startup Projects**.
2. Select **Multiple startup projects**.
3. Set both **ChangeStreamManager** and **CDCTarget** to *Start*.
4. Press **F5** to run the cluster.

You will see two console windows appear. The rapid log output proves that data is successfully flowing from MongoDB, through Kafka, and into the target environment.

### 3. Monitor the Internal Migration Dashboard
Open a web browser and navigate to:
```text
http://localhost:5000/index.html
```
*(Note: Port number may vary based on your `launchSettings.json`).*
* Verify the connection status shows **🟢 LIVE STREAM CONNECTED**.
* Watch the scrolling terminal feed and total event counter update in real-time via SignalR as the Go script writes to MongoDB.

### 4. Act as the Downstream Consumer
Open a new browser tab and navigate to the external Gateway API interface:
```text
http://localhost:5000/swagger
```
Follow this sequence to test external consumption:
1. **Subscribe:** Expand `POST /api/CDCTargetApi/subscribe`. Click *Try it out*, provide a sample `clientName` and `topicName` (e.g., `cdc_benchmark`), and click *Execute* to perform the initial handshake.
2. **Verify Logs:** Expand `GET /api/CDCTargetApi/logs` and click *Execute* to verify the backend SQLite database formally audited the subscription request.
3. **Consume Data:** Expand `GET /api/CDCTargetApi/events` and click *Execute*. Scroll down to view the processed, real-time JSON payload containing the most recent database operations. Re-execute after a few seconds to verify the timestamps and Document IDs update live.
