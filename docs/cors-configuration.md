# CORS configuration

The API uses JWT bearer tokens in the `Authorization` header. Its
`ConfiguredOrigins` CORS policy therefore allows only explicitly configured
browser origins and does not enable browser credentials.

## Development

`media-vault-app.API/appsettings.Development.json` includes the current local
browser origins:

- `https://localhost:61366` for the Vite web development server;
- `http://localhost:3000`, `http://localhost:5173`, and `http://localhost:8081`
  for supported local browser development servers.

The API's local URL (`http://localhost:5210`) is a server URL, not a CORS
origin. Native Android requests also do not use browser CORS, so emulator and
physical-device API URLs such as `http://10.0.2.2:5210` or a reachable LAN
address do not belong in `AllowedOrigins`. A browser-based client must add its
own exact origin through environment-specific configuration.

## Production

Production must provide at least one exact origin through deployment
configuration. The checked-in example uses a non-secret placeholder:

```text
Cors__AllowedOrigins__0=https://your-production-web.example
```

Add additional origins with `Cors__AllowedOrigins__1`, and so on. Values must
contain only the `http` or `https` scheme, host, and optional port. Do not add a
trailing slash, path, query, fragment, credentials, wildcard, or an arbitrary
origin. Production startup validation rejects missing or malformed values
before the API serves traffic. A non-production environment with no origins
configured starts with CORS denied, which is useful for isolated test hosts.

The policy allows the methods and headers needed by bearer-token preflight
requests for controller routes and the development `/openapi/v1.json` endpoint.
It intentionally does not call `AllowCredentials()`; enabling that later would
require a separately reviewed authentication and CORS change.
