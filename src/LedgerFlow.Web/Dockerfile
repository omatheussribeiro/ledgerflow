FROM node:24-alpine AS build
WORKDIR /app
COPY src/LedgerFlow.Web/package*.json ./
RUN npm ci
COPY src/LedgerFlow.Web/ .
RUN npm run build

FROM nginx:1.29-alpine AS final
COPY src/LedgerFlow.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/web/browser /usr/share/nginx/html
EXPOSE 80
HEALTHCHECK --interval=15s --timeout=3s --retries=3 CMD wget -q -O /dev/null http://127.0.0.1/health || exit 1
