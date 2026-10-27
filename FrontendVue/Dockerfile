# ---------- Etapa 1: compilar el frontend Vue ----------
# Se usa Debian (slim) y no Alpine para evitar problemas de binarios nativos
# de Vite/Rollup (musl) al instalar dependencias.
FROM node:22-slim AS build
WORKDIR /app

# En Docker la API se consume por /api (nginx la redirige al backend)
ENV VITE_API_URL=/api

# Instalar dependencias (usa --legacy-peer-deps por el conflicto de oxlint)
COPY package*.json ./
RUN npm install --legacy-peer-deps

# Copiar el resto y compilar a /app/dist
COPY . ./
RUN npm run build

# ---------- Etapa 2: servir con nginx ----------
FROM nginx:alpine AS runtime
# Configuración que sirve la SPA y redirige /api al backend
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
