# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files
COPY OnlineSurvey.sln .
COPY src/OnlineSurvey.Web/OnlineSurvey.Web.csproj src/OnlineSurvey.Web/

# Restore
RUN dotnet restore src/OnlineSurvey.Web/OnlineSurvey.Web.csproj

# Copy source code
COPY src/OnlineSurvey.Web/ src/OnlineSurvey.Web/

# Build and publish
WORKDIR /src/src/OnlineSurvey.Web
RUN dotnet publish -c Release -o /app/publish

# Runtime stage - using nginx to serve static files
FROM nginx:alpine AS final

# Copy nginx configuration first
COPY src/OnlineSurvey.Web/nginx.conf /etc/nginx/nginx.conf

# Copy published Blazor app
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .

# Ensure proper permissions for nginx user
RUN chmod -R 755 /usr/share/nginx/html && \
    find /usr/share/nginx/html -type f -exec chmod 644 {} \; && \
    chown -R nginx:nginx /usr/share/nginx/html && \
    chown -R nginx:nginx /var/cache/nginx && \
    chown -R nginx:nginx /var/log/nginx && \
    chown -R nginx:nginx /etc/nginx && \
    touch /var/run/nginx.pid && \
    chown nginx:nginx /var/run/nginx.pid

EXPOSE 80

# Switch to non-root user
USER nginx

CMD ["nginx", "-g", "daemon off;"]
