using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Extensions.Logging.NUnit;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Quibble.Xunit;
using RichardSzalay.MockHttp;
using Smith.MatrixSdk.ApiTypes;
using Smith.MatrixSdk.Extensions;
using Smith.MatrixSdk.Tests.TestFramework;
using static Smith.MatrixSdk.Tests.TestFramework.TestConstants;

namespace Smith.MatrixSdk.Tests
{
    public class SyncTests : HttpMockTestBase
    {
        private const string AccessToken = "myAccessToken";
        private static readonly TimeSpan LongPollingTimeout = TimeSpan.FromMinutes(1.0);
        private static readonly SyncResponse EmptySyncResponse = new(
            NextBatch: "",
            Rooms: null,
            Presence: null,
            AccountData: null
        );

        private static readonly ILogger Logger = new NUnitLogger(nameof(SyncTests));

        [Test]
        public async Task TimeoutPassingTest()
        {
            HttpHandler.Expect(MatrixApiUris.Sync)
                .Respond("application/json", request =>
                {
                    var query = request.RequestUri.NotNull().Query;
                    var requestValues = QueryHelpers.ParseQuery(query);
                    var requestContent = new SyncRequest(
                        Filter: requestValues.TryGetValue("filter", out var filter) ? (string)filter : null,
                        Since: requestValues.TryGetValue("since", out var since) ? (string)since : null,
                        FullState: requestValues.TryGetValue("full_state", out var fullState)
                            ? fullState == "true"
                            : null,
                        SetPresence: requestValues.TryGetValue("set_presence", out var setPresence)
                            ? Enum.Parse<SetPresence>(setPresence)
                            : null,
                        Timeout: requestValues.TryGetValue("timeout", out var timeout)
                            ? int.Parse(timeout, CultureInfo.InvariantCulture)
                            : null
                    );

                    Assert.AreEqual(new SyncRequest(
                        Filter: null,
                        Since: null,
                        FullState: null,
                        SetPresence: null,
                        Timeout: (int)LongPollingTimeout.TotalMilliseconds
                    ), requestContent);

                    return EmptySyncResponse.SerializeToStream();
                });

            using var httpClient = HttpHandler.ToHttpClient();
            var client = new MatrixClient(Logger, httpClient, HomeserverUri);

            await client.StartEventPolling(AccessToken, LongPollingTimeout).FirstAsync();
        }

        [Test]
        public async Task SincePassingTest()
        {
            const string nextBatchId = "nextBatchId";

            HttpHandler.Expect(MatrixApiUris.Sync)
                .Respond("application/json", _ =>
                {
                    var response = EmptySyncResponse with {NextBatch = nextBatchId};
                    return response.SerializeToStream();
                });

            HttpHandler.Expect(MatrixApiUris.Sync)
                .Respond("application/json", request =>
                {
                    var since = QueryHelpers.ParseQuery(request.RequestUri.NotNull().Query)["since"];
                    Assert.AreEqual(nextBatchId, (string)since);
                    return EmptySyncResponse.SerializeToStream();
                });

            using var httpClient = HttpHandler.ToHttpClient();
            var client = new MatrixClient(Logger, httpClient, HomeserverUri);

            await client.StartEventPolling(AccessToken, TimeSpan.Zero).Take(2);
        }

        [Test]
        public async Task SyncResponseDeserializationTest()
        {
            // Taken from the Matrix documentation:
            const string syncResponse = @"{
  ""next_batch"": ""s72595_4483_1934"",
  ""presence"": {
    ""events"": [
      {
        ""content"": {
          ""avatar_url"": ""mxc://localhost:wefuiwegh8742w"",
          ""last_active_ago"": 2478593,
          ""presence"": ""online"",
          ""currently_active"": false,
          ""status_msg"": ""Making cupcakes""
        },
        ""type"": ""m.presence"",
        ""sender"": ""@example:localhost""
      }
    ]
  },
  ""account_data"": {
    ""events"": [
      {
        ""type"": ""org.example.custom.config"",
        ""content"": {
          ""custom_config_key"": ""custom_config_value""
        }
      }
    ]
  },
  ""rooms"": {
    ""join"": {
      ""!726s6s6q:example.com"": {
        ""summary"": {
          ""m.heroes"": [
            ""@alice:example.com"",
            ""@bob:example.com""
          ],
          ""m.joined_member_count"": 2,
          ""m.invited_member_count"": 0
        },
        ""state"": {
          ""events"": [
            {
              ""content"": {
                ""membership"": ""join"",
                ""avatar_url"": ""mxc://example.org/SEsfnsuifSDFSSEF"",
                ""displayname"": ""Alice Margatroid""
              },
              ""type"": ""m.room.member"",
              ""event_id"": ""$143273582443PhrSn:example.org"",
              ""room_id"": ""!726s6s6q:example.com"",
              ""sender"": ""@example:example.org"",
              ""origin_server_ts"": 1432735824653,
              ""unsigned"": {
                ""age"": 1234
              },
              ""state_key"": ""@alice:example.org""
            }
          ]
        },
        ""timeline"": {
          ""events"": [
            {
              ""content"": {
                ""membership"": ""join"",
                ""avatar_url"": ""mxc://example.org/SEsfnsuifSDFSSEF"",
                ""displayname"": ""Alice Margatroid""
              },
              ""type"": ""m.room.member"",
              ""event_id"": ""$143273582443PhrSn:example.org"",
              ""room_id"": ""!726s6s6q:example.com"",
              ""sender"": ""@example:example.org"",
              ""origin_server_ts"": 1432735824653,
              ""unsigned"": {
                ""age"": 1234
              },
              ""state_key"": ""@alice:example.org""
            },
            {
              ""content"": {
                ""body"": ""This is an example text message"",
                ""msgtype"": ""m.text"",
                ""format"": ""org.matrix.custom.html"",
                ""formatted_body"": ""<b>This is an example text message</b>""
              },
              ""type"": ""m.room.message"",
              ""event_id"": ""$143273582443PhrSn:example.org"",
              ""room_id"": ""!726s6s6q:example.com"",
              ""sender"": ""@example:example.org"",
              ""origin_server_ts"": 1432735824653,
              ""unsigned"": {
                ""age"": 1234
              }
            }
          ],
          ""limited"": true,
          ""prev_batch"": ""t34-23535_0_0""
        },
        ""ephemeral"": {
          ""events"": [
            {
              ""content"": {
                ""user_ids"": [
                  ""@alice:matrix.org"",
                  ""@bob:example.com""
                ]
              },
              ""type"": ""m.typing"",
              ""room_id"": ""!jEsUZKDJdhlrceRyVU:example.org""
            }
          ]
        },
        ""account_data"": {
          ""events"": [
            {
              ""content"": {
                ""tags"": {
                  ""u.work"": {
                    ""order"": 0.9
                  }
                }
              },
              ""type"": ""m.tag""
            },
            {
              ""type"": ""org.example.custom.room.config"",
              ""content"": {
                ""custom_config_key"": ""custom_config_value""
              }
            }
          ]
        }
      }
    },
    ""invite"": {
      ""!696r7674:example.com"": {
        ""invite_state"": {
          ""events"": [
            {
              ""sender"": ""@alice:example.com"",
              ""type"": ""m.room.name"",
              ""state_key"": """",
              ""content"": {
                ""name"": ""My Room Name""
              }
            },
            {
              ""sender"": ""@alice:example.com"",
              ""type"": ""m.room.member"",
              ""state_key"": ""@bob:example.com"",
              ""content"": {
                ""membership"": ""invite""
              }
            }
          ]
        }
      }
    },
    ""leave"": {}
  }
}";

            HttpHandler.Expect(MatrixApiUris.Sync)
                .Respond("application/json", _ => syncResponse.ToStream());

            using var httpClient = HttpHandler.ToHttpClient();
            var client = new MatrixClient(Logger, httpClient, HomeserverUri);

            var response = await client.StartEventPolling(AccessToken, LongPollingTimeout).FirstAsync();

            var expected = new SyncResponse(
                NextBatch: "s72595_4483_1934",
                Presence: new Presence(
                    Events: new[]
                    {
                        new Event(
                            Content: new JObject
                            {
                                ["avatar_url"] = "mxc://localhost:wefuiwegh8742w",
                                ["last_active_ago"] = 2478593,
                                ["presence"] = "online",
                                ["currently_active"] = false,
                                ["status_msg"] = "Making cupcakes"
                            },
                            Type: "m.presence",
                            AdditionalData: new Dictionary<string, JToken>
                            {
                                ["sender"] = "@example:localhost"
                            }
                        )
                    }
                ),
                AccountData: new AccountData(
                    Events: new[]
                    {
                        new Event(
                            Type: "org.example.custom.config",
                            Content: JObject.Parse(@"{
                                ""custom_config_key"": ""custom_config_value""
                            }")
                        )
                    }
                ),
                Rooms: new Rooms(
                    Join: new Dictionary<string, JoinedRoom>
                    {
                        ["!726s6s6q:example.com"] = new(
                            Summary: new RoomSummary(
                                Heroes: new[]
                                {
                                    "@alice:example.com",
                                    "@bob:example.com"
                                },
                                JoinedMemberCount: 2,
                                InvitedMemberCount: 0
                            ),
                            State: new State(
                                Events: new[]
                                {
                                    new StateEvent(
                                        Content: JObject.Parse(@"{
                                            ""membership"": ""join"",
                                            ""avatar_url"": ""mxc://example.org/SEsfnsuifSDFSSEF"",
                                            ""displayname"": ""Alice Margatroid""
                                        }"),
                                        Type: "m.room.member",
                                        EventId: "$143273582443PhrSn:example.org",
                                        Sender: "@example:example.org",
                                        OriginServerTs: 1432735824653L,
                                        Unsigned: new UnsignedData(
                                            Age: 1234,
                                            RedactedBecause: null,
                                            TransactionId: null
                                        ),
                                        PrevContent: null,
                                        StateKey: "@alice:example.org",
                                        AdditionalData: new Dictionary<string, JToken>
                                        {
                                            ["room_id"] = "!726s6s6q:example.com",
                                        }
                                    )
                                }
                            ),
                            Timeline: new Timeline(
                                Events: new[]
                                {
                                    new RoomEvent(
                                        Content: JObject.Parse(@"{
                                            ""membership"": ""join"",
                                            ""avatar_url"": ""mxc://example.org/SEsfnsuifSDFSSEF"",
                                            ""displayname"": ""Alice Margatroid""
                                        }"),
                                        Type: "m.room.member",
                                        EventId: "$143273582443PhrSn:example.org",
                                        Sender: "@example:example.org",
                                        OriginServerTs: 1432735824653L,
                                        Unsigned: new UnsignedData(
                                            Age: 1234,
                                            RedactedBecause: null,
                                            TransactionId: null
                                        ),
                                        AdditionalData: new Dictionary<string, JToken>
                                        {
                                            ["room_id"] = "!726s6s6q:example.com",
                                            ["state_key"] = "@alice:example.org"
                                        }
                                    ),
                                    new RoomEvent(
                                        Content: JObject.Parse(@"{
                                            ""body"": ""This is an example text message"",
                                            ""msgtype"": ""m.text"",
                                            ""format"": ""org.matrix.custom.html"",
                                            ""formatted_body"": ""<b>This is an example text message</b>""
                                        }"),
                                        Type: "m.room.message",
                                        EventId: "$143273582443PhrSn:example.org",
                                        Sender: "@example:example.org",
                                        OriginServerTs: 1432735824653,
                                        Unsigned: new UnsignedData(
                                            Age: 1234,
                                            RedactedBecause: null,
                                            TransactionId: null
                                        ),
                                        AdditionalData: new Dictionary<string, JToken>
                                        {
                                            ["room_id"] = "!726s6s6q:example.com",
                                            ["unsigned"] = new JObject
                                            {
                                                ["age"] = 1234
                                            }
                                        }
                                    )
                                },
                                Limited: true,
                                PrevBatch: "t34-23535_0_0"
                            ),
                            Ephemeral: new Ephemeral(
                                Events: new []
                                {
                                    new Event(
                                        Content: JObject.Parse(@"{
                                            ""user_ids"": [
                                                ""@alice:matrix.org"",
                                                ""@bob:example.com""
                                            ]
                                        }"),
                                        Type: "m.typing",
                                        AdditionalData: new Dictionary<string, JToken>
                                        {
                                            ["room_id"] = "!jEsUZKDJdhlrceRyVU:example.org"
                                        }
                                    )
                                }
                            ),
                            AccountData: new AccountData(
                                Events: new[]
                                {
                                    new Event(
                                        Content: JObject.Parse(@"{
                                            ""tags"": {
                                                ""u.work"": {
                                                    ""order"": 0.9
                                                }
                                            }
                                        }"),
                                        Type: "m.tag"
                                    ),
                                    new Event(
                                        Type: "org.example.custom.room.config",
                                        Content: JObject.Parse(@"{
                                            ""custom_config_key"": ""custom_config_value""
                                        }")
                                    )
                                }
                            ),
                            UnreadNotifications: null
                        )
                    },
                    Invite: new Dictionary<string, InvitedRoom>
                    {
                        ["!696r7674:example.com"] = new(
                            InviteState: new InviteState(
                                Events: new[]
                                {
                                    new StrippedState(
                                        Sender: "@alice:example.com",
                                        Type: "m.room.name",
                                        StateKey: "",
                                        Content: JObject.Parse(@"{
                                            ""name"": ""My Room Name""
                                        }")
                                    ),
                                    new StrippedState(
                                        Sender: "@alice:example.com",
                                        Type: "m.room.member",
                                        StateKey: "@bob:example.com",
                                        Content: JObject.Parse(@"{
                                            ""membership"": ""invite""
                                        }")
                                    ),
                                }
                            )
                        )
                    },
                    Leave: new Dictionary<string, LeftRoom>()
                )
            );

            JsonAssert.Equal(expected.SerializeToString(), response.SerializeToString());
        }

        [Test]
        public async Task AuthorizationHeaderTest()
        {
            HttpHandler.Expect(MatrixApiUris.Sync)
                .Respond("application/json", request =>
                {
                    var auth = request.Headers.Authorization.NotNull();
                    Assert.AreEqual("Bearer", auth.Scheme);
                    Assert.AreEqual(AccessToken, auth.Parameter);

                    return EmptySyncResponse.SerializeToStream();
                });

            using var httpClient = HttpHandler.ToHttpClient();
            var client = new MatrixClient(Logger, httpClient, HomeserverUri);

            await client.StartEventPolling(AccessToken, LongPollingTimeout).FirstAsync();
        }

        [Test]
        public void HttpErrorTest()
        {
            HttpHandler.Expect(MatrixApiUris.Sync)
                .Respond(_ => new HttpResponseMessage(HttpStatusCode.Gone));

            using var httpClient = HttpHandler.ToHttpClient();
            var client = new MatrixClient(Logger, httpClient, HomeserverUri);

            var exception = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await client.StartEventPolling(AccessToken, LongPollingTimeout).FirstAsync());
            Assert.AreEqual(HttpStatusCode.Gone, exception.StatusCode);
        }

        [Test]
        public void CancellationTest()
        {
            var response = EmptySyncResponse with
            {
                NextBatch = "123"
            };

            var cts = new CancellationTokenSource();

            var httpHandler = new MockHttpMessageHandler();
            httpHandler.Expect(MatrixApiUris.Sync)
                .Respond("application/json", _ => response.SerializeToStream());

            using var httpClient = httpHandler.ToHttpClient();
            var client = new MatrixClient(Logger, httpClient, HomeserverUri);

            client.StartEventPolling(AccessToken, TimeSpan.Zero)
                .Do(r =>
                {
                    Assert.AreEqual(response, r);

                    if (cts.IsCancellationRequested)
                        Assert.Fail("Request made after cancellation");

                    cts.Cancel();
                })
                .Subscribe(cts.Token);

            Assert.True(cts.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1.0)));
        }
    }
}
