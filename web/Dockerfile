# syntax=docker/dockerfile:1
FROM node:20-alpine AS build
WORKDIR /src
COPY web/package.json web/package-lock.json ./
RUN npm ci
COPY web/ ./
ARG VITE_GOOGLE_MAPS_API_KEY=""
ENV VITE_API_BASE_URL=""
ENV VITE_GOOGLE_MAPS_API_KEY=$VITE_GOOGLE_MAPS_API_KEY
RUN npm run build

FROM nginx:1.27-alpine AS runtime
# API_HOST lets a second (e.g. trial) stack point this web image at its own
# api container instead of the default "api" service name — the official
# nginx image auto-substitutes env vars into templates/*.template on start.
ENV API_HOST=api
COPY web/nginx.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /src/dist /usr/share/nginx/html
EXPOSE 80
