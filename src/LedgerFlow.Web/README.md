# LedgerFlow Web

Angular 22 client for LedgerFlow. Components are standalone and keep TypeScript, HTML and SCSS in separate files.

## API configuration

HTTP calls use `environment.apiUrl`. Both committed environments currently target the local API:

```ts
apiUrl: 'http://localhost:5080/'
```

- `src/environments/environment.ts` is the production build default.
- `src/environments/environment.development.ts` replaces it for `ng serve` through `angular.json`.
- `ApiService` owns financial endpoints.
- `AuthStore` owns authentication endpoints.
- `authInterceptor` adds the JWT only to protected requests.

The API returns successful calls as `{ success, message, data }`. Core services unwrap `data` before exposing observables to components. Errors remain RFC 7807 `ProblemDetails` and include `message`, `code` and `traceId`.

Accounts, categories and transactions support creation and inline editing through reactive forms. Deletion asks for confirmation and calls the API's logical-delete endpoint; the item disappears from active UI collections while its database history remains intact.

Edit and delete actions share accessible icon-and-text buttons with distinct colors, hover/focus feedback, tooltips and resource-specific screen-reader labels.

Update the production environment URL before deploying the SPA to a topology where the browser cannot reach `localhost:5080`.

## Development

Start the API on port `5080`, then run:

```bash
npm ci
npm start
```

Open <http://localhost:4200>.

## Quality checks

```bash
npm run lint
npm test -- --watch=false
npm run build
```

The Vitest suite covers the application shell, API URL/query construction, financial create/update/delete requests, login, registration, logout/session persistence and JWT interceptor behavior.

## Component convention

Each visual component uses three files:

```text
feature.component.ts    # behavior and state
feature.component.html  # Angular template
feature.component.scss  # component styles
```

Inline `template` and `styles` metadata are intentionally avoided.
