using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanchoMultiplayerBot.Database.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialLobby : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                -- 1. Configuración inicial de la sala #1 en "LobbyConfigurations"
                INSERT INTO "LobbyConfigurations" (
                    "Id",
                    "Name",
                    "Mode",
                    "TeamMode",
                    "ScoreMode",
                    "Size",
                    "Password",
                    "Mods",
                    "Behaviours"
                ) OVERRIDING SYSTEM VALUE VALUES (
                    1,
                    'Auto Host Rotate Ratita [4.0* - 5.5*] | osu!std',
                    0,
                    0,
                    0,
                    16,
                    '',
                    ARRAY['Freemod'],
                    ARRAY[
                        'HostQueueBehavior',
                        'MapManagerBehavior',
                        'AutoStartBehavior',
                        'AntiAfkBehavior',
                        'FunCommandsBehavior',
                        'AbortVoteBehavior',
                        'DynamicRoomBehavior'
                    ]
                )
                ON CONFLICT ("Id") DO NOTHING;

                -- Ajustar la secuencia de IDs de LobbyConfigurations
                SELECT setval(pg_get_serial_sequence('"LobbyConfigurations"', 'Id'), coalesce(max("Id"), 1)) FROM "LobbyConfigurations";

                -- 2. Configuración inicial de cada Behavior en "LobbyBehaviorConfig"
                INSERT INTO "LobbyBehaviorConfig" ("LobbyConfigurationId", "BehaviorName", "Data")
                SELECT 1, 'MapManagerBehaviorConfig', '{
                    "LimitStarRating": true,
                    "MinimumStarRating": 4.0,
                    "MaximumStarRating": 5.5,
                    "StarRatingErrorMargin": 0.3,
                    "LimitMapLength": true,
                    "MinimumMapLength": 30,
                    "MaximumMapLength": 330,
                    "AutomaticallySkipHostViolations": true,
                    "MaximumHostViolations": 3,
                    "AllowDoubleTime": true
                }'
                WHERE NOT EXISTS (
                    SELECT 1 FROM "LobbyBehaviorConfig"
                    WHERE "LobbyConfigurationId" = 1 AND "BehaviorName" = 'MapManagerBehaviorConfig'
                );

                INSERT INTO "LobbyBehaviorConfig" ("LobbyConfigurationId", "BehaviorName", "Data")
                SELECT 1, 'AutoStartBehaviorConfig', '{
                    "AllPlayersReadyStart": true,
                    "NewMapTimer": 90,
                    "StartEarlyWarning": 10,
                    "StartTimerMinimumSeconds": 1,
                    "StartTimerMaximumSeconds": 300
                }'
                WHERE NOT EXISTS (
                    SELECT 1 FROM "LobbyBehaviorConfig"
                    WHERE "LobbyConfigurationId" = 1 AND "BehaviorName" = 'AutoStartBehaviorConfig'
                );

                INSERT INTO "LobbyBehaviorConfig" ("LobbyConfigurationId", "BehaviorName", "Data")
                SELECT 1, 'FunCommandsBehaviorConfig', '{
                    "AnnounceLeaderboardScores": true
                }'
                WHERE NOT EXISTS (
                    SELECT 1 FROM "LobbyBehaviorConfig"
                    WHERE "LobbyConfigurationId" = 1 AND "BehaviorName" = 'FunCommandsBehaviorConfig'
                );

                INSERT INTO "LobbyBehaviorConfig" ("LobbyConfigurationId", "BehaviorName", "Data")
                SELECT 1, 'DynamicRoomBehaviorConfig', '{
                    "StarRatingIncrease": 3.0,
                    "StarRatingDecrease": 2.0
                }'
                WHERE NOT EXISTS (
                    SELECT 1 FROM "LobbyBehaviorConfig"
                    WHERE "LobbyConfigurationId" = 1 AND "BehaviorName" = 'DynamicRoomBehaviorConfig'
                );

                -- 3. Mensajes informativos de rotación en "NoticeMessages" (AnnouncementBehavior)
                INSERT INTO "NoticeMessages" ("Id", "Message") OVERRIDING SYSTEM VALUE VALUES
                    (1, 'Welcome to Auto Host Rotate! Use !help, !queue or !regulations for lobby information.'),
                    (2, 'Feeling like spectating? Use !autoskip on to automatically pass your host turn.'),
                    (3, 'Having issues downloading a map? Use !mirror for alternative download links (BeatConnect, osu.direct, Mino).'),
                    (4, 'Did someone go AFK as host? Use !skip to vote skip, or wait for the bot auto-kick.')
                ON CONFLICT ("Id") DO NOTHING;

                SELECT setval(pg_get_serial_sequence('"NoticeMessages"', 'Id'), coalesce(max("Id"), 1)) FROM "NoticeMessages";

                -- 4. Usuarios administradores iniciales en "Users"
                UPDATE "Users" SET "Administrator" = true WHERE "Name" IN ('AutohostApp', 'maxxi02ASS');

                INSERT INTO "Users" ("Name", "Administrator", "Playtime", "MatchesPlayed", "NumberOneResults", "AutoSkipEnabled")
                SELECT 'AutohostApp', true, 0, 0, 0, false
                WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "Name" = 'AutohostApp');

                INSERT INTO "Users" ("Name", "Administrator", "Playtime", "MatchesPlayed", "NumberOneResults", "AutoSkipEnabled")
                SELECT 'maxxi02ASS', true, 0, 0, 0, false
                WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "Name" = 'maxxi02ASS');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "LobbyBehaviorConfig"
                WHERE "LobbyConfigurationId" = 1
                  AND "BehaviorName" IN (
                      'MapManagerBehaviorConfig',
                      'AutoStartBehaviorConfig',
                      'FunCommandsBehaviorConfig',
                      'DynamicRoomBehaviorConfig'
                  );

                DELETE FROM "NoticeMessages" WHERE "Id" IN (1, 2, 3, 4);

                DELETE FROM "LobbyConfigurations" WHERE "Id" = 1;

                DELETE FROM "Users" WHERE "Name" IN ('AutohostApp', 'maxxi02ASS') AND "MatchesPlayed" = 0;
                """);
        }
    }
}

