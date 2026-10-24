# Organization Wallet and Identus Boundary

The organization wallet is browser-controlled. It generates a P-256 `did:key` and
keeps the private key in memory only. The API stores only `client_did`; encrypted
backup export is an explicit user action and is never uploaded to the API.

The local Cloud Agent 1.39 API does not provide a holder wallet or a way to turn
an arbitrary browser `did:key` into a managed PRISM DID. The organization DID is
therefore not created with `did-registrar/dids`, and the CNE flow never creates a
second CNE-owned identity for the organization.

Credential issuance is attempted only when all three configuration values are
present: `OrganizationSchemaId`, `OrganizationHolderConnectionId`, and
`OrganizationIssuingDid`. The offer uses Identus' documented connection-based
`issue-credentials/credential-offers` endpoint. The holder must still accept the
offer; until that is verified, the API remains `credential_pending` and the
organization is not marked approved. With the current local setup, the required
configuration is absent, so the API exposes the exact pending setup instead of
fabricating a credential, issuer reference, or completion state.
