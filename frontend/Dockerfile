# syntax=docker/dockerfile:1

# ---------- build ----------
FROM node:22-alpine AS build
WORKDIR /app

# Instalar dependencias (capa cacheable).
COPY package*.json ./
RUN npm ci

# Compilar el frontend (usa rutas /api relativas; Nginx hace el proxy).
COPY . .
RUN npm run build

# ---------- runtime ----------
FROM nginx:alpine AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
