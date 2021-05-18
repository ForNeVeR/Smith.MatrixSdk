using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Smith.MatrixSdk.ApiTypes
{
    /// <param name="Filter">
    /// The ID of a filter created using the filter API or a filter JSON object encoded as a string. The server will
    /// detect whether it is an ID or a JSON object by whether the first character is a <c>"{"</c> open brace. Passing
    /// the JSON inline is best suited to one off requests. Creating a filter using the filter API is recommended for
    /// clients that reuse the same filter multiple times, for example in long poll requests.
    /// </param>
    /// <param name="Since">A point in time to continue a sync from.</param>
    /// <param name="FullState">
    /// <para>Controls whether to include the full state for all rooms the user is a member of.</para>
    /// <para>
    ///     If this is set to <c>true</c>, then all state events will be returned, even if <paramref name="Since"/> is
    ///     non-empty. The timeline will still be limited by the <paramref name="Since"/> parameter. In this case, the
    ///     <paramref name="Timeout"/> parameter will be ignored and the query will return immediately, possibly with an
    ///     empty timeline.
    /// </para>
    /// <para>
    ///     If <c>false</c>, and <paramref name="Since"/> is non-empty, only state which has changed since the point
    ///     indicated by <paramref name="Since"/> will be returned.
    /// </para>
    /// <para>By default, this is <c>false</c>.</para>
    /// </param>
    /// <param name="SetPresence">
    ///	Controls whether the client is automatically marked as online by polling this API. If this parameter is omitted
    /// then the client is automatically marked as online when it uses this API. Otherwise if the parameter is set to
    /// <c>"offline"</c> then the client is not marked as being online when it uses this API. When set to
    /// <c>"unavailable"</c>, the client is marked as being idle. One of: <c>["offline", "online", "unavailable"]</c>
    /// </param>
    /// <param name="Timeout">
    /// <para>
    ///     The maximum time to wait, in milliseconds, before returning this request. If no events (or other data)
    ///     become available before this time elapses, the server will return a response with empty fields.
    /// </para>
    /// <para>By default, this is 0, so the server will return immediately even if the response is empty.</para>
    /// </param>
    public record SyncRequest
    (
        string? Filter = null,
        string? Since = null,
        bool? FullState = null,
        SetPresence? SetPresence = null,
        int? Timeout = null
    );

    public enum SetPresence
    {
        Offline,
        Online,
        Unavailable
    }

    /// <param name="NextBatch">
    /// Required. The batch token to supply in the <c>since</c> param of the next <c>/sync</c> request.
    /// </param>
    /// <param name="Rooms">Updates to rooms.</param>
    /// <param name="Presence">The updates to the presence status of other users.</param>
    /// <param name="AccountData">The global private data created by this user.</param>
    public record SyncResponse
    (
        string NextBatch,
        Rooms? Rooms,
        Presence? Presence,
        AccountData? AccountData
    );

    /// <param name="Join">The rooms that the user has joined, mapped as room ID to room information.</param>
    /// <param name="Invite">The rooms that the user has been invited to, mapped as room ID to room information.</param>
    /// <param name="Leave">
    /// The rooms that the user has left or been banned from, mapped as room ID to room information.
    /// </param>
    public record Rooms
    (
        IReadOnlyDictionary<string, JoinedRoom>? Join,
        IReadOnlyDictionary<string, InvitedRoom>? Invite,
        IReadOnlyDictionary<string, LeftRoom>? Leave
    );

    /// <param name="Events">List of events.</param>
    public record Presence(IReadOnlyCollection<Event>? Events);

    /// <param name="Events">List of events.</param>
    public record AccountData(IReadOnlyCollection<Event>? Events);

    /// <param name="Summary">Information about the room which clients may need to correctly render it to users.</param>
    /// <param name="State">
    /// <para>
    ///     Updates to the state, between the time indicated by the <c>since</c> parameter, and the start of the
    ///     <paramref name="Timeline"/> (or all state up to the start of the <paramref name="Timeline"/>, if
    ///     <c>since</c> is not given, or <c>full_state</c> is true).
    /// </para>
    /// </param>
    /// <param name="Timeline">The timeline of messages and state changes in the room.</param>
    /// <param name="Ephemeral">
    /// The ephemeral events in the room that aren't recorded in the timeline or state of the room. e.g. typing.
    /// </param>
    /// <param name="AccountData">The private data that this user has attached to this room.</param>
    /// <param name="UnreadNotifications">Counts of unread notifications for this room.</param>
    public record JoinedRoom
    (
        RoomSummary? Summary,
        State? State,
        Timeline? Timeline,
        Ephemeral? Ephemeral,
        AccountData? AccountData,
        UnreadNotificationCounts? UnreadNotifications
    );

    /// <param name="InviteState">
    /// The state of a room that the user has been invited to. These state events may only have the <c>sender</c>,
    /// <c>type</c>, <c>state_key</c> and <c>content</c> keys present. These events do not replace any state that the
    /// client already has for the room, for example if the client has archived the room. Instead the client should
    /// keep two separate copies of the state: the one from the <c>invite_state</c> and one from the archived
    /// <c>state</c>. If the client joins the room then the current state will be given as a delta against the
    /// archived <c>state</c> not the <c>invite_state</c>.
    /// </param>
    public record InvitedRoom(InviteState? InviteState);

    /// <param name="State">The state updates for the room up to the start of the timeline.</param>
    /// <param name="Timeline">
    /// The timeline of messages and state changes in the room up to the point when the user left.
    /// </param>
    /// <param name="AccountData">The private data that this user has attached to this room.</param>
    public record LeftRoom(State? State, Timeline? Timeline, AccountData? AccountData);

    /// <param name="Content">Required. The fields in this object will vary depending on the type of event.</param>
    /// <param name="Type">
    /// Required. The type of event. This SHOULD be namespaced similar to Java package naming conventions e.g.
    /// <c>'com.example.subdomain.event.type'</c>
    /// </param>
    /// <param name="AdditionalData">Additional event fields (not deserialized into other fields).</param>
    public record Event(
        JObject Content,
        string Type,
        [property: JsonExtensionData] IDictionary<string, JToken>? AdditionalData = null
    );

    /// <param name="Heroes">
    /// <para>
    ///     The users which can be used to generate a room name if the room does not have one. Required if the room's
    ///     <c>m.room.name</c> or <c>m.room.canonical_alias</c> state events are unset or empty.
    /// </para>
    /// <para>
    ///     should be the first 5 members of the room, ordered by stream ordering, which are joined or invited. The
    ///     list must never include the client's own user ID. When no joined or invited members are available, this
    ///     should consist of the banned and left users. More than 5 members may be provided, however less than 5
    ///     should only be provided when there are less than 5 members to represent.
    /// </para>
    /// <para>
    ///     When lazy-loading room members is enabled, the membership events for the heroes MUST be included in the
    ///     <c>state</c>, unless they are redundant. When the list of users changes, the server notifies the client by
    ///     sending a fresh list of heroes. If there are no changes since the last sync, this field may be omitted.
    /// </para>
    /// </param>
    /// <param name="JoinedMemberCount">
    /// The number of users with <c>membership</c> of <c>join</c>, including the client's own user ID. If this field
    /// has not changed since the last sync, it may be omitted. Required otherwise.
    /// </param>
    /// <param name="InvitedMemberCount">
    /// The number of users with <c>membership</c> of <c>invite</c>. If this field has not changed since the last
    /// sync, it may be omitted. Required otherwise.
    /// </param>
    public record RoomSummary
    (
        [JsonProperty("m.heroes")] IReadOnlyCollection<string>? Heroes,
        [JsonProperty("m.joined_member_count")] int? JoinedMemberCount,
        [JsonProperty("m.invited_member_count")] int? InvitedMemberCount
    );

    /// <param name="Events">List of events.</param>
    public record State(IReadOnlyCollection<StateEvent>? Events);

    /// <param name="Events">List of events.</param>
    /// <param name="Limited">
    /// True if the number of events returned was limited by the <c>limit</c> on the filter.
    /// </param>
    /// <param name="PrevBatch">
    /// A token that can be supplied to the <c>from</c> parameter of the <c>rooms/{roomId}/messages</c> endpoint.
    /// </param>
    public record Timeline(IReadOnlyCollection<RoomEvent>? Events, bool? Limited, string? PrevBatch);

    /// <param name="Events">List of events.</param>
    public record Ephemeral(IReadOnlyCollection<Event>? Events);

    /// <param name="HighlightCount">
    /// The number of unread notifications for this room with the highlight flag set
    /// </param>
    /// <param name="NotificationCount">The total number of unread notifications for this room</param>
    public record UnreadNotificationCounts(int? HighlightCount, int? NotificationCount);

    /// <param name="Events">The <see cref="StrippedState"/> events that form the invite state.</param>
    public record InviteState(IReadOnlyCollection<StrippedState>? Events);

    /// <param name="Content">Required. The fields in this object will vary depending on the type of event.</param>
    /// <param name="Type">
    /// Required. The type of event. This SHOULD be namespaced similar to Java package naming conventions e.g.
    /// <c>'com.example.subdomain.event.type'</c>
    /// </param>
    /// <param name="EventId">Required. The globally unique event identifier.</param>
    /// <param name="Sender">Required. Contains the fully-qualified ID of the user who sent this event.</param>
    /// <param name="OriginServerTs">
    /// Required. Timestamp in milliseconds on originating homeserver when this event was sent.
    /// </param>
    /// <param name="Unsigned">Contains optional extra information about the event.</param>
    /// <param name="PrevContent">
    /// Optional. The previous <c>content</c> for this event. If there is no previous content, this key will be missing.
    /// </param>
    /// <param name="StateKey">
    /// Required. A unique key which defines the overwriting semantics for this piece of room state. This value is
    /// often a zero-length string. The presence of this key makes this event a State Event. State keys starting with
    /// an @ are reserved for referencing user IDs, such as room members. With the exception of a few events, state
    /// events set with a given user's ID as the state key MUST only be set by that user.
    /// </param>
    /// <param name="AdditionalData">Additional event fields (not deserialized into other fields).</param>
    public record StateEvent
    (
        JObject Content,
        string Type,
        string EventId,
        string Sender,
        long OriginServerTs,
        UnsignedData? Unsigned,
        JObject? PrevContent,
        string StateKey,
        [property: JsonExtensionData] IDictionary<string, JToken>? AdditionalData = null
    );

    /// <param name="Content">
    /// Required. The fields in this object will vary depending on the type of event. When interacting with the REST
    /// API, this is the HTTP body.
    /// </param>
    /// <param name="Type">
    /// Required. The type of event. This SHOULD be namespaced similar to Java package naming conventions e.g.
    /// <c>'com.example.subdomain.event.type'</c>
    /// </param>
    /// <param name="EventId">Required. The globally unique event identifier.</param>
    /// <param name="Sender">Required. Contains the fully-qualified ID of the user who sent this event.</param>
    /// <param name="OriginServerTs">
    /// Required. Timestamp in milliseconds on originating homeserver when this event was sent.
    /// </param>
    /// <param name="Unsigned">Contains optional extra information about the event.</param>
    /// <param name="AdditionalData">Additional event fields (not deserialized into other fields).</param>
    public record RoomEvent
    (
        JObject Content,
        string Type,
        string EventId,
        string Sender,
        long OriginServerTs,
        UnsignedData? Unsigned,
        [property: JsonExtensionData] IDictionary<string, JToken>? AdditionalData = null
    );

    /// <param name="Content">Required. The <c>content</c> for the event.</param>
    /// <param name="StateKey">Required. The <c>state_key</c> for the event.</param>
    /// <param name="Type">Required. The <c>type</c> for the event.</param>
    /// <param name="Sender">Required. The <c>sender</c> for the event.</param>
    public record StrippedState
    (
        JObject Content,
        string StateKey,
        string Type,
        string Sender
    );

    /// <param name="Age">
    /// The time in milliseconds that has elapsed since the event was sent. This field is generated by the local
    /// homeserver, and may be incorrect if the local time on at least one of the two servers is out of sync, which
    /// can cause the age to either be negative or greater than it actually is.
    /// </param>
    /// <param name="RedactedBecause">Optional. The event that redacted this event, if any.</param>
    /// <param name="TransactionId">
    /// The client-supplied transaction ID, if the client being given the event is the same one which sent it.
    /// </param>
    public record UnsignedData
    (
        long? Age,
        Event? RedactedBecause,
        string? TransactionId
    );
}
