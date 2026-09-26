# Endpoint Inventory API Contract

## Agent authentication

Agent uses a dedicated device credential. The server must bind the credential to `DeviceKey`; the client cannot submit inventory for another device.

## POST /api/security/endpoints/heartbeat

Payload:
- DeviceKey
- ComputerName
- SerialNumber
- OsName
- OsVersion
- EmployeeCode
- EquipmentAssetId
- AgentVersion

Purpose: upsert endpoint and update LastSeenUtc.

## POST /api/security/endpoints/inventory

Payload:
- DeviceKey
- Computer metadata
- Software[]
- Services[]

Rules:
1. Authenticate agent.
2. Resolve DeviceKey from credential.
3. Upsert endpoint.
4. Replace current software/service snapshot transactionally, or upsert with a collection timestamp and retire entries missing from the latest complete snapshot.
5. Never execute commands received in inventory payload.
6. Run compliance after a complete inventory snapshot.
7. Create/update alerts idempotently.

## GET /api/security/endpoints/{deviceKey}/policy

Returns only effective policies applicable to the endpoint's scope. Does not return arbitrary commands.

## GET /api/security/endpoints/{deviceKey}/compliance

Returns current compliance summary and open alerts.

## Security requirements

- HTTPS only.
- Device credential rotation/revocation.
- Replay protection using timestamp/nonce where supported.
- Payload size limits.
- Rate limiting.
- Audit log for registration, credential rotation, policy changes and exception approval.
- EmployeeCode and EquipmentAssetId are server-resolved when possible; never trust client scope claims.
