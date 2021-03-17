Smith.MatrixSdk [![Status Enfer][status-enfer]][andivionian-status-classifier]
===============

SDK for [Matrix][matrix] network API.

Documentation
-------------

- [Changelog][changelog]
- [License][license] (MIT)

Quick start
-----------

Please see [`Smith.MatrixSdk.Example.Simplified` project][example.simplified]
for a working example.

Features
--------

This library implements the following features from the [Matrix Client-Server
API r0.6.1][matrix.spec]:

- [ ] 2 API Standards
    - [ ] Error response recognition
    - [ ] 2.1 GET `/_matrix/client/versions`
- [x] 3 ~~Web Browser Clients~~: not applicable
- [ ] 4 Server Discovery
- [ ] 5 Client Authentication
    - [ ] Device support
    - [ ] 5.3 Soft logout
    - [ ] 5.4 User-Interactive Authentication API
        - [ ] 5.4.2 User-interactive API in the REST API
        - [ ] 5.4.4 Authentication types
            - [ ] 5.4.4.1 Password-based
                - [x] Basic (deprecated) support
                - [ ] `identifier` field
            - [ ] 5.4.4.2 Google ReCaptcha
            - [ ] 5.4.4.3 Token-based
            - [ ] 5.4.4.4 OAuth2-based
            - [ ] 5.4.4.5 Single Sign-On
            - [ ] 5.4.4.6 Email-based (identity / homeserver)
            - [ ] 5.4.4.7 Phone number/MSISDN-based (identity / homeserver)
            - [ ] 5.4.4.8 Dummy Auth authentication
        - [ ] 5.4.5 Fallback
        - [ ] 5.4.6 Identifier types
    - [ ] 5.5 Login
        - [ ] 5.5.1 GET `/_matrix/client/r0/login`
        - [ ] 5.5.2 POST `/_matrix/client/r0/login`
            - [x] Basic (deprecated) version (`user` field)
            - [ ] Request fields: `identifier`, `token`, `device_id`,
              `initial_device_display_name`
            - [ ] Response fields: `device_id`, `well_known`
                - [ ] Remove `refresh_token` support
        - [ ] 5.5.3 POST `/_matrix/client/r0/logout`
        - [ ] 5.5.4 POST `/_matrix/client/r0/logout/all`
        - [ ] 5.5.5 Login Fallback
    - [ ] 5.6 Account registration and management
    - [ ] 5.7 Adding Account Administrative Contact Information
    - [ ] 5.8 Current account information
- [ ] 6 Capabilities negotiation
- [ ] 7 Pagination
- [ ] 8 Filtering
- [ ] 9 Events
- [ ] 10 Rooms
- [ ] 11 User Data
- [ ] 12 Security
- [ ] 13 Modules

Build
-----

To build Smith.MatrixSdk, you'll need [.NET Core SDK][dotnet] 5.0 or later.

```
$ dotnet build --configuration Release
```

(replace `Release` with `Debug` for debug build)

Documentation
-------------

- [Changelog][changelog]
- [License][license]

[changelog]: ./CHANGELOG.md
[example.simplified]: ./Smith.MatrixSdk.Example.Simplified/Program.cs
[license]: ./LICENSE.md
[matrix.spec]: https://matrix.org/docs/spec/client_server/r0.6.1
[status-enfer]: https://img.shields.io/badge/status-enfer-orange.svg

[andivionian-status-classifier]: https://github.com/ForNeVeR/andivionian-status-classifier#status-enfer-
