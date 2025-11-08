com.flowcast.sdk/
├─ package.json
├─ README.md
├─ CHANGELOG.md
├─ LICENSE
├─ link.xml
├─ Editor/
│  ├─ Flowcast.Core.Editor.asmdef
│  ├─ Flowcast.Rest.Editor.asmdef
│  ├─ Core/                // env inspectors, shared drawers/validators
│  └─ Rest/Workbench/      // Postman-like Workbench, record/replay (REST)
├─ Runtime/
│  ├─ Flowcast.Core.asmdef
│  ├─ Flowcast.Rest.asmdef
│  ├─ Flowcast.Realtime.asmdef
│  ├─ Core/
│  │  ├─ Common/           // shared models & primitives (see below)
│  │  ├─ Environments/     // env assets, selectors, secrets
│  │  ├─ Policies/         // retry, circuit, rate, backoff
│  │  ├─ Auth/             // IAuthProvider, ITokenStore (generic)
│  │  ├─ Serialization/    // ISerializer + registry (media-type aware)
│  │  ├─ Cache/            // ICacheProvider (memory/persistent), cache policies
│  │  ├─ Diagnostics/      // ILogger, metrics hooks, correlation id
│  │  ├─ Concurrency/      // IClock, IRandom, async helpers, single-flight
│  │  └─ Security/         // request signing policy, cert pinning (generic)
│  ├─ Rest/
│  │  ├─ Client/           // RestClient, RequestExecutor, fluent builder
│  │  ├─ Pipeline/         // IPipelineBehavior for REST (auth, cache, logs, etc.)
│  │  ├─ Transport/        // HTTP transports (UnityWebRequest, HttpClient), handlers
│  │  ├─ ContentTypes/     // REST-specific serializers/adapters if any
│  │  └─ Handlers/         // IRequestHandler<TReq,TRes> domain handlers (REST)
│  └─ Realtime/
│     ├─ Client/           // Realtime client façade(s)
│     ├─ Transport/        // WebSocket/UDP transports
│     ├─ Protocols/        // message models, framing, heartbeats
│     ├─ Pipeline/         // reuse Core policies; realtime-specific behaviors
│     └─ Features/         // channels/rooms, presence, reconnection logic
├─ Samples~/
│  ├─ MinimalRestSetup/
│  └─ RealtimeEcho/
├─ Documentation~/
└─ Tests/
   ├─ EditMode/
   └─ PlayMode/
