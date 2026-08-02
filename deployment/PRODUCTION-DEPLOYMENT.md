# ORMS Production Deployment

## Prerequisites

- Windows 10/11 or Windows Server with Docker Desktop or Docker Engine
- A fixed local-network IP address for the ORMS host
- Xprinter installed on the cashier workstation or print-service host
- Regular PostgreSQL backup storage

## Initial deployment

1. Copy `.env.production.example` to `.env`.
2. Replace every placeholder secret.
3. Run `docker compose -f docker-compose.production.yml up -d --build`.
4. Confirm `/api/system/health/live` returns `Healthy`.
5. Confirm `/api/system/health/ready` returns `Ready`.
6. Restrict the ORMS port to the trusted local network.
7. Create the first administrator through the approved bootstrap process.
8. Configure store records, printers, tax settings, and user roles.

## Database backup

Example manual backup:

```bash
docker compose -f docker-compose.production.yml exec -T database \
  pg_dump -U outfitters -d outfitters -Fc > outfitters-backup.dump
```

Test restoration regularly on a separate database.

## Upgrade procedure

1. Create and verify a database backup.
2. Pull the approved release commit.
3. Run the automated build and tests.
4. Rebuild containers.
5. Apply EF Core migrations using the approved deployment account.
6. Verify health endpoints, login, POS checkout, inventory, and printing.
7. Keep the prior image available for rollback.

## Minimum production checklist

- Replace all example passwords and JWT secrets.
- Enable HTTPS through a reverse proxy when traffic leaves the trusted LAN.
- Limit database access to the application host.
- Use named employee accounts instead of shared logins.
- Review audit logs routinely.
- Configure daily database backups and off-device retention.
- Test Xprinter output before opening the store.
