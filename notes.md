```mermaid
    graph TD
        subgraph PearlMetric [PearlMetric Analytical Data Pipeline]
            UI[Angular 22 Frontend]
            Node[Node 24 Gateway]
            NET[.NET 10 GatewayApi]
            Py[Python CV Worker]
            DB[(PostgreSQL 18 DB)]
        end

        UI -->|HTTP Requests| Node
        Node -->|Reverse Proxy| NET
        NET -->|Internal HTTP Call| Py
        NET -->|Entity Framework Core| DB
        
        style UI fill:#dd0031,stroke:#fff,stroke-width:2px,color:#fff
        style Node fill:#339933,stroke:#fff,stroke-width:2px,color:#fff
        style NET fill:#512bd4,stroke:#fff,stroke-width:2px,color:#fff
        style Py fill:#3776ab,stroke:#fff,stroke-width:2px,color:#fff
        style DB fill:#336791,stroke:#fff,stroke-width:2px,color:#fff
```
```mermaid
    graph LR
        subgraph Infra [Backend Infrastructure]
            App[.NET API AppPool]
            Scheduler{Quartz.NET Engine}
            DB[(PostgreSQL 18 Ledger)]
            Notify[Notification Worker]
        end

        App -->|Schema Updates| DB
        Scheduler -->|Persistent State| DB
        Scheduler -->|Trigger Job| Notify
        
        style App fill:#512bd4,stroke:#fff,stroke-width:2px,color:#fff
        style Scheduler fill:#61dafb,stroke:#333,stroke-width:2px,color:#333
        style DB fill:#336791,stroke:#fff,stroke-width:2px,color:#fff
        style Notify fill:#ff9900,stroke:#fff,stroke-width:2px,color:#fff
````