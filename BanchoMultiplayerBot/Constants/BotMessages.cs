#if !INTERNAL_BOT_MESSAGES
using BanchoMultiplayerBot.Notifications;
#endif

namespace BanchoMultiplayerBot.Constants;

#if INTERNAL_BOT_MESSAGES
internal static class BotMessages
#else
public static class BotMessages
#endif
{
    public static class Logs
    {
        public static class Host
        {
            public const string UnhandledException = "[Host] Excepción fatal no controlada en el dominio de la aplicación";
            public const string StartingEnvironment = "[Host] Iniciando BanchoMultiplayerBot en el entorno '{Environment}'";
            public const string MigratingDatabase = "[Host] Verificando y aplicando migraciones pendientes en la base de datos...";
            public const string DatabaseMigrationCompleted = "[Host] Migraciones de base de datos verificadas y aplicadas correctamente";
            public const string DatabaseMigrationFailed = "[Host] Error crítico al aplicar las migraciones de la base de datos";
            public const string WatchdogDisconnectedFatal = "[BotHealthService] El bot lleva más de {DisconnectedSeconds} segundos desconectado de Bancho. Deteniendo la aplicación para forzar un reinicio en frío...";
            public const string TrackerMultiplayerLobbyNullOnStart = "[LobbyTrackerService] No se pudo iniciar el seguimiento del lobby #{LobbyId} porque la instancia MultiplayerLobby es nula";
            public const string TrackerEventProcessorNullOnStart = "[LobbyTrackerService] No se pudo iniciar el seguimiento del lobby #{LobbyId} porque BehaviorEventProcessor es nulo";
            public const string TrackerStarted = "[LobbyTrackerService] Seguimiento en tiempo real iniciado para el lobby #{LobbyId} (Canal: '{Channel}')";
            public const string TrackerMultiplayerLobbyNullOnStop = "[LobbyTrackerService] Omitiendo desuscripción del lobby #{LobbyId} al detenerse porque MultiplayerLobby no estaba activo";
            public const string TrackerEventProcessorNullOnStop = "[LobbyTrackerService] Omitiendo desuscripción del lobby #{LobbyId} al detenerse porque BehaviorEventProcessor no estaba activo";
            public const string TrackerStopped = "[LobbyTrackerService] Seguimiento en tiempo real detenido para el lobby #{LobbyId} (Canal: '{Channel}')";
            public const string AuthFailedNotAdmin = "[Auth] Inicio de sesión OAuth rechazado para el usuario '{Username}' (ID: '{UserId}'): no posee permisos de administrador";
            public const string AuthSucceeded = "[Auth] Usuario administrador '{Username}' (ID: '{UserId}') autenticado exitosamente en el panel web";
            public const string HubClientConnected = "[LobbyEventHub] Cliente web conectado al hub SignalR (ConnectionId: '{ConnectionId}')";
            public const string HubClientDisconnected = "[LobbyEventHub] Cliente web desconectado del hub SignalR (ConnectionId: '{ConnectionId}')";
        }

        public static class Bot
        {
            public const string Starting = "[Bot] Iniciando servicios del bot (Usuario IRC: '{Username}')...";
            public const string ShuttingDown = "[Bot] Apagando el bot y cerrando {LobbyCount} lobby(s) configurado(s)...";
            public const string LoadingLobbyConfigurations = "[Bot] Cargando configuraciones de lobbies desde la base de datos...";
            public const string LoadedLobbyConfiguration = "[Bot] Configuración de lobby cargada: #{LobbyConfigurationId} ('{LobbyName}')";
            public const string StartingLobby = "[Bot] Iniciando conexión para el lobby #{LobbyConfigurationId}...";
            public const string LobbyReadyTimeout = "[Bot] El lobby #{LobbyConfigurationId} no logró estar listo tras 10 segundos de espera (Estado actual: {LobbyHealth})";
            public const string LobbyDeleted = "[Bot] El lobby #{LobbyConfigurationId} fue cerrado y eliminado correctamente";
        }

        public static class Lobby
        {
            public const string DisposingPreviousInstance = "[Lobby #{LobbyId}] Ya existe una instancia activa en el canal '{Channel}', cerrando instancia previa antes de reconectar...";
            public const string JoiningExistingChannel = "[Lobby #{LobbyId}] Intentando reconectar al canal existente '{ExistingChannel}' para la sala '{LobbyName}'...";
            public const string CreatingNewChannel = "[Lobby #{LobbyId}] Solicitando a Bancho la creación de un nuevo canal multijugador para la sala '{LobbyName}'...";
            public const string ConfigurationNotFound = "[Lobby #{LobbyId}] Error crítico: no se encontró la configuración del lobby en la base de datos";
            public const string InstanceBuilt = "[Lobby #{LobbyId}] Instancia del lobby '{LobbyName}' construida e inicializada exitosamente en el canal '{Channel}'";
            public const string InstanceShutdown = "[Lobby #{LobbyId}] Instancia del lobby cerrada correctamente (Canal: '{Channel}')";
            public const string BanchoClientNullOnCreate = "[Lobby #{LobbyId}] BanchoClient es nulo durante el evento de creación del lobby";
            public const string BanchoClientNullOnJoin = "[Lobby #{LobbyId}] BanchoClient es nulo al procesar la unión exitosa al canal '{Channel}'";
            public const string ChannelJoinedBuilding = "[Lobby #{LobbyId}] Unión exitosa al canal '{Channel}', construyendo instancia del lobby...";
            public const string BanchoClientNullOnJoinFailure = "[Lobby #{LobbyId}] BanchoClient es nulo al procesar el fallo de unión al canal '{AttemptedChannel}'";
            public const string IgnoringUnrelatedChannelJoinFailure = "[Lobby #{LobbyId}] Ignorando fallo de unión al canal '{AttemptedChannel}' (canal esperado: '{ExpectedChannel}')";
            public const string ChannelJoinFailedCreatingNew = "[Lobby #{LobbyId}] No se pudo unir al canal previo '{AttemptedChannel}'. Creando un nuevo canal para la sala '{LobbyName}'...";
        }

        public static class Bancho
        {
            public const string Connecting = "[BanchoConnection] Conectando al servidor IRC de Bancho con el usuario '{Username}'...";
            public const string ConnectionException = "[BanchoConnection] Excepción durante la conexión con Bancho para el usuario '{Username}'";
            public const string ConnectionTerminated = "[BanchoConnection] La conexión activa con Bancho ha finalizado";
            public const string Disconnecting = "[BanchoConnection] Cerrando conexión activa con Bancho...";
            public const string Disconnected = "[BanchoConnection] Desconectado de Bancho correctamente";
            public const string Authenticated = "[BanchoConnection] Autenticado exitosamente en Bancho como '{Username}'";
            public const string ConnectionLostReconnecting = "[BanchoConnection] Se perdió la conexión con Bancho. Esperando {ReconnectDelaySeconds} segundos antes de iniciar la reconexión...";
            public const string ReconnectAttempt = "[BanchoConnection] Intentando reconectar a Bancho (Intento {Attempt}/{MaxAttempts})...";
            public const string ReconnectedSuccessfully = "[BanchoConnection] Reconexión a Bancho completada con éxito en el intento {Attempt}/{MaxAttempts}";
            public const string ReconnectAttemptFailed = "[BanchoConnection] Falló el intento de reconexión {Attempt}/{MaxAttempts}. Reintentando en {AttemptDelaySeconds} segundos...";
            public const string ReconnectExhaustedFatal = "[BanchoConnection] No se pudo reconectar a Bancho tras agotar los {MaxAttempts} intentos configurados. Cerrando el cliente...";

            public const string ChannelRuntimeIdParsed = "[ChannelHandler] ID numérico registrado para el canal '{Channel}': {RuntimeId}";
            public const string ChannelIdParseError = "[ChannelHandler] Error al extraer el ID de sala desde el mensaje IRC '{RawMessage}'";

            public const string CommandExecuting = "[CommandHandler] Ejecutando comando '{Command}' en el canal '{Channel}'";
            public const string CommandSendTimeout = "[CommandHandler] Tiempo de espera agotado (5s) al intentar enviar el comando '{Command}' al canal '{Channel}'";
            public const string CommandRetrying = "[CommandHandler] Sin respuesta de Bancho para el comando '{Command}' en '{Channel}'. Reintentando envío ({Attempt}/{MaxAttempts})...";
            public const string CommandResponseTimeout = "[CommandHandler] Falló la ejecución del comando '{Command}' en '{Channel}' tras {MaxAttempts} intentos sin respuesta";

            public const string WatchdogStarting = "[ConnectionHandler] Iniciando tarea de monitoreo (watchdog) de conexión TCP/IRC...";
            public const string WatchdogStopping = "[ConnectionHandler] Deteniendo tarea de monitoreo (watchdog) de conexión...";
            public const string WatchdogNotRunningOnStop = "[ConnectionHandler] Se solicitó detener el watchdog de conexión, pero la tarea no estaba activa (Estado: {TaskStatus})";
            public const string WatchdogStarted = "[ConnectionHandler] Tarea de monitoreo (watchdog) de conexión iniciada correctamente";
            public const string WatchdogDetectedLostConnection = "[ConnectionHandler] ¡Se detectó la pérdida de conexión con el servidor IRC de Bancho!";
            public const string WatchdogStopped = "[ConnectionHandler] Tarea de monitoreo (watchdog) de conexión detenida";
            public const string WatchdogTcpUnhealthy = "[ConnectionHandler] El socket TCP hacia Bancho reporta estado desconectado o inválido";
            public const string WatchdogSendingKeepAlive = "[ConnectionHandler] No se recibieron mensajes de Bancho en los últimos 5 minutos. Enviando mensaje de prueba a BanchoBot para verificar la conexión...";

            public const string MessagePumpStarting = "[MessageHandler] Iniciando cola de procesamiento de mensajes IRC...";
            public const string MessagePumpStopping = "[MessageHandler] Deteniendo cola de procesamiento de mensajes IRC...";
            public const string MessagePumpNotRunningOnStop = "[MessageHandler] Se solicitó detener la cola de mensajes, pero la tarea no estaba activa (Estado: {TaskStatus})";
            public const string MessagePumpStarted = "[MessageHandler] Cola de procesamiento de mensajes IRC iniciada (Límite: {BurstCount} mensajes cada {WindowSeconds}s)";
            public const string MessageDroppedTooLong = "[MessageHandler] Mensaje descartado hacia '{Channel}' por superar el límite de {MaxLength} caracteres (Longitud: {ActualLength})";
            public const string MessageSendFailed = "[MessageHandler] Error al enviar el mensaje '{MessageContent}' al canal '{Channel}'";
            public const string MessagePumpStopped = "[MessageHandler] Cola de procesamiento de mensajes IRC detenida";
            public const string IrcMessageReceived = "[MessageHandler] [{Recipient}] {Sender}: {Content}";
            public const string IrcMessageSent = "[MessageHandler] [{Recipient}] {Sender}: {Content}";
        }

        public static class Dispatcher
        {
            public const string BehaviorNotFound = "[BehaviorEventProcessor] En el lobby #{LobbyId} se intentó registrar el behavior '{Behavior}', pero no existe ninguna clase que lo implemente";
            public const string BehaviorRegistered = "[BehaviorEventProcessor] Behavior '{BehaviorName}' registrado correctamente en el lobby #{LobbyId}";
            public const string BehaviorWorkerStarting = "[BehaviorEventProcessor] Iniciando worker de eventos para '{Behavior}' en el lobby #{LobbyId}...";
            public const string BehaviorCallbackTimeout = "[BehaviorEventProcessor] Tiempo de espera agotado (15s) ejecutando {CallbackName}.{MethodName}() en el lobby #{LobbyId}";
            public const string BehaviorCallbackException = "[BehaviorEventProcessor] Excepción no controlada ejecutando {CallbackName}.{MethodName}() en el lobby #{LobbyId}";
            public const string BehaviorWorkerStopped = "[BehaviorEventProcessor] Worker de eventos para '{Behavior}' detenido en el lobby #{LobbyId}";

            public const string RegisteringCommands = "[CommandProcessor] Registrando comandos de jugadores disponibles...";
            public const string CommandInstanceCreationFailed = "[CommandProcessor] No se pudo crear la instancia del comando '{CommandType}'";
            public const string CommandRegistered = "[CommandProcessor] Comando '{CommandType}' registrado exitosamente";
            public const string CommandExecuting = "[CommandProcessor] Ejecutando comando '!{Command}' solicitado por '{Sender}' en '{Channel}'";
            public const string CommandLobbyExecutionError = "[CommandProcessor] Error al ejecutar el comando '!{Command}' de '{Sender}' en el lobby '{Lobby}'";
            public const string CommandGlobalExecutionError = "[CommandProcessor] Error al ejecutar el comando '!{Command}' de '{Sender}' en contexto global";

            public const string SendMessageLobbyNull = "[BehaviorEventContext] Intento de enviar mensaje en el lobby #{LobbyId} cuando MultiplayerLobby es nulo";
            public const string ConfigNotFoundCreatingDefault = "[BehaviorConfigProvider] No se encontró configuración previa para '{BehaviorDataType}' en el lobby #{LobbyId}. Creando configuración por defecto...";
            public const string DataNotFoundCreatingDefault = "[BehaviorDataProvider] No se encontraron datos previos para '{BehaviorDataType}' en el lobby #{LobbyId}. Inicializando estado limpio...";

            public const string TimerStopNotRunning = "[TimerProvider] En el lobby #{LobbyId} se intentó detener el proveedor de temporizadores cuando no estaba en ejecución";
            public const string TimerStopWorkerNull = "[TimerProvider] En el lobby #{LobbyId} se intentó detener el proveedor de temporizadores pero la tarea worker es nula";
            public const string TimerStopWorkerTimeout = "[TimerProvider] La tarea worker de temporizadores del lobby #{LobbyId} no finalizó dentro del límite de 2 segundos";
            public const string TimerExpiredMarkedInactive = "[TimerProvider] El temporizador '{Name}' del lobby #{LobbyId} fue marcado como inactivo porque expiró hace más de 45 segundos";
            public const string TimersLoaded = "[TimerProvider] Se cargaron {Count} temporizador(es) desde la base de datos para el lobby #{LobbyId}";
            public const string TimerWorkerStarted = "[TimerProvider] Worker de temporizadores iniciado para el lobby #{LobbyId}";
            public const string TimerEarlyWarningTriggered = "[TimerProvider] Aviso previo activado para el temporizador '{Name}' en el lobby #{LobbyId}";
            public const string TimerEarlyWarningDispatcherNull = "[TimerProvider] BehaviorEventProcessor es nulo en el lobby #{LobbyId}, no se puede disparar el aviso previo del temporizador '{Name}'";
            public const string TimerElapsedTriggered = "[TimerProvider] Temporizador '{Name}' finalizado en el lobby #{LobbyId}";
            public const string TimerElapsedDispatcherNull = "[TimerProvider] BehaviorEventProcessor es nulo en el lobby #{LobbyId}, no se puede disparar el evento del temporizador '{Name}'";
            public const string TimerWorkerStopped = "[TimerProvider] Worker de temporizadores detenido para el lobby #{LobbyId}";
            public const string TimerStarting = "[Timer] Iniciando temporizador '{Name}' con duración de {Duration}";
            public const string TimerStopping = "[Timer] Deteniendo temporizador '{Name}'";

            public const string VoteExpiredAborted = "[VoteProvider] La votación '{VoteName}' del lobby #{LobbyId} llevaba activa más de 2 minutos y fue cancelada";
            public const string VotesLoaded = "[VoteProvider] Se cargaron {VoteCount} votación(es) desde la base de datos para el lobby #{LobbyId}";
            public const string VoteLobbyNull = "[Vote] No se pudo registrar el voto en '{Vote}' porque MultiplayerLobby es nulo en el lobby #{LobbyId}";
            public const string VotePassed = "[Vote] La votación '{Vote}' en el lobby #{LobbyId} fue aprobada ({VoteCount}/{RequiredVotes} votos)";
            public const string VoteAborted = "[Vote] Votación '{Vote}' reiniciada/cancelada en el lobby #{LobbyId} (tenía {Votes} voto(s))";
            public const string VoteRemovedDisconnectedPlayer = "[Vote] Se eliminó el voto del jugador desconectado '{PlayerName}' de la votación '{Vote}' en el lobby #{LobbyId}";
        }

        public static class Behaviors
        {
            public const string AutoStartIgnoredNoPlayers = "[AutoStartBehavior] Ignorando temporizador de inicio automático en el lobby #{LobbyId} porque no hay jugadores en la sala";
            public const string AutoStartIgnoredChangingMap = "[AutoStartBehavior] Ignorando temporizador de inicio automático en el lobby #{LobbyId} porque el host está cambiando el mapa";
            public const string AutoStartIgnoredInvalidDuration = "[AutoStartBehavior] Ignorando solicitud de temporizador de inicio en el lobby #{LobbyId} por duración fuera de rango ({Duration})";

            public const string BanResettingJoinFrequency = "[BanBehavior] Reiniciando contador de ingresos en el lobby #{LobbyId} para el usuario sancionado '{User}' (>15 min desde su último ingreso)";
            public const string BannedUserJoinedRoom = "[BanBehavior] El usuario sancionado '{User}' ingresó al lobby #{LobbyId} (Ingreso #{Frequency} en ventana reciente, acción: '!mp {Method}')";
            public const string PlayerBanned = "[BanBehavior] Sanción aplicada en el lobby #{LobbyId} por '{Admin}': jugador '{User}', tipo '{BanType}', duración {Days} día(s), motivo: '{Reason}'";
            public const string MapsetBanned = "[BanBehavior] El mapset #{MapSetId} fue baneado en el lobby #{LobbyId} por el administrador '{Admin}'";

            public const string DataProviderLeaderboardLookupFailed = "[DataProviderBehavior] Falló la consulta del leaderboard en osu! API para el mapa #{BeatmapId} en el lobby #{LobbyId}: {Error}";
            public const string DataProviderLeaderboardZeroScores = "[DataProviderBehavior] La consulta del leaderboard en osu! API devolvió 0 puntajes para el mapa #{BeatmapId} en el lobby #{LobbyId}";
            public const string DataProviderFetchingPlayerScore = "[DataProviderBehavior] Consultando puntaje reciente en osu! API para el jugador '{Name}' (Lobby #{LobbyId})";

            public const string DynamicRoomCalculatedMetrics = "[DynamicRoomBehavior] Cálculo post-partida en lobby #{LobbyId}: factorDificultad={MapDifficultyFactor:0.00}, éxitoPromedio={AverageSuccess:0.00}, ajusteSR={SrChange:+0.00;-0.00;0.00}";
            public const string DynamicRoomNewTargetSr = "[DynamicRoomBehavior] Nuevo objetivo de dificultad para el lobby #{LobbyId}: {StarRatingTarget:0.00}*";

            public const string FunCommandsGlobalTagsLookupFailed = "[FunCommandsBehavior] Falló la obtención del catálogo global de etiquetas (tags) desde osu! API en el lobby #{LobbyId}";
            public const string FunCommandsLeaderboardScoreFound = "[FunCommandsBehavior] El jugador '{PlayerName}' obtuvo el puesto #{Placement} en el leaderboard global del mapa (ScoreId: {ScoreId}, Lobby #{LobbyId})";
            public const string FunCommandsIgnoringMismatchedScore = "[FunCommandsBehavior] Ignorando puntaje {ScoreId} en el lobby #{LobbyId} porque el BeatmapId del puntaje no coincide con la partida ({ScoreBeatmapId} != {GameBeatmapId})";
            public const string FunCommandsStoredGameScores = "[FunCommandsBehavior] Se almacenaron {ScoreCount} puntaje(s) para la partida #{GameId} en el lobby #{LobbyId}";
            public const string FunCommandsStoreScoreException = "[FunCommandsBehavior] Excepción al guardar los puntajes de la partida #{GameId} en la base de datos (Lobby #{LobbyId})";

            public const string HealthMonitorLobbyDead = "[HealthMonitorBehavior] No se recibieron eventos en el lobby #{LobbyId} ('{Channel}') durante más de 2 horas (Último evento: {LastEventTime:u}). Recreando sala...";
            public const string HealthMonitorRecreateFailed = "[HealthMonitorBehavior] Error al intentar reconectar/recrear el lobby #{LobbyId} tras inactividad";

            public const string HostQueuePlayerBannedSkipping = "[HostQueueBehavior] El jugador '{PlayerName}' posee una restricción activa en el lobby #{LobbyId}, omitiendo su ingreso a la cola de host";
            public const string HostQueuePlayerAlreadyInQueue = "[HostQueueBehavior] El jugador '{PlayerName}' ya figuraba en la cola del lobby #{LobbyId} durante el evento de ingreso";
            public const string HostQueuePlayerNotInQueueOnDisconnect = "[HostQueueBehavior] El jugador '{PlayerName}' se desconectó del lobby #{LobbyId} pero no figuraba en la cola de host";
            public const string HostQueueCandidateCheckException = "[HostQueueBehavior] Excepción al verificar si el jugador '{PlayerName}' es candidato válido a host en el lobby #{LobbyId}";

            public const string MapManagerBeatmapNotFound = "[MapManagerBehavior] El mapa #{BeatmapId} seleccionado en el lobby #{LobbyId} no existe en osu!, restaurando mapa de respaldo #{FallbackId}";
            public const string MapManagerGetBeatmapFailed = "[MapManagerBehavior] Error de osu! API al obtener datos del mapa #{BeatmapId} en el lobby #{LobbyId}: {ApiError}";
            public const string MapManagerGetAttributesFailed = "[MapManagerBehavior] Error de osu! API al obtener atributos de dificultad del mapa #{BeatmapId} en el lobby #{LobbyId}: {ApiError}";
            public const string MapManagerApiTimeout = "[MapManagerBehavior] Tiempo de espera agotado en osu! API al consultar el mapa #{BeatmapId} en el lobby #{LobbyId}";
            public const string MapManagerUnexpectedException = "[MapManagerBehavior] Excepción inesperada al consultar el mapa #{BeatmapId} en el lobby #{LobbyId}";
            public const string MapManagerEnforcingRegulations = "[MapManagerBehavior] Validando reglamento para el mapa #{BeatmapId} en el lobby #{LobbyId}: estado = {MapStatus}";
            public const string MapManagerInvalidPlayingMapAborted = "[MapManagerBehavior] ¡Se detectó un intento de iniciar partida con un mapa/mods fuera del rango permitido en el lobby #{LobbyId} (Mapa #{BeatmapId}, Host: '{HostName}')! Abortando partida...";

            public const string RoomManagerStuckMatchAborted = "[RoomManagerBehavior] Se detectó una partida posiblemente colgada en el lobby #{LobbyId} ('{Channel}', {FinishedCount}/{TotalPlayers} jugadores finalizaron). Abortando partida...";
        }

        public static class Osu
        {
            public const string PpMapRequestFailed = "[PerformancePointService] La solicitud de PP para el mapa #{BeatmapId} falló con código de estado HTTP {StatusCode}";
            public const string PpMapParseFailed = "[PerformancePointService] No se pudo deserializar la respuesta del servicio de PP para el mapa #{BeatmapId}";
            public const string PpMapCalculationError = "[PerformancePointService] No se pudo calcular el PP del mapa #{BeatmapId}: {ErrorMessage}";
            public const string PpScoreRequestFailed = "[PerformancePointService] La solicitud de PP para el puntaje #{ScoreId} (Mapa #{BeatmapId}) falló con código de estado HTTP {StatusCode}";
            public const string PpScoreParseFailed = "[PerformancePointService] No se pudo deserializar la respuesta del servicio de PP para el puntaje #{ScoreId}";
            public const string PpScoreCalculationError = "[PerformancePointService] No se pudo calcular el PP del puntaje #{ScoreId} (Mapa #{BeatmapId}): {ErrorMessage}";
            public const string PpServiceNotAvailable = "[PerformancePointService] El servicio de cálculo de PP no está configurado o no se encuentra disponible";
            public const string BeatmapCacheOutdated = "[PerformancePointService] El archivo en caché del mapa #{BeatmapId} está desactualizado ({LastUpdatedTime:u} > {LastDownloadedTime:u}), descargando actualización...";
            public const string BeatmapDownloadFailed = "[PerformancePointService] No se pudo descargar el archivo .osu del mapa #{BeatmapId}: {ErrorMessage}";
            public const string UnknownModAbbreviation = "[ScoreExtensions] Abreviatura de mod desconocida al calcular bitset: '{ModAbbreviation}'";
            public const string RecentScoreUserLookupFailed = "[RecentScoreCommand] Falló la búsqueda en osu! API del usuario '{Sender}': {ApiError}";
            public const string RecentScoreBeatmapLookupFailed = "[RecentScoreCommand] Falló la búsqueda en osu! API del mapa #{BeatmapId} para '{Sender}': {ApiError}";
        }

        public static class Notifications
        {
            public const string WebhookUrlNotConfigured = "[NotificationManager] URL de Webhook de Discord no configurada o inválida. Las notificaciones de Discord estarán desactivadas";
            public const string NotificationDeduplicated = "[NotificationManager] Notificación omitida por deduplicación activa (Clave: '{DeduplicationKey}', Título: '{Title}')";
            public const string NotificationSendFailed = "[NotificationManager] Error al enviar la notificación '{Title}' mediante {ProviderName}: {ErrorMessage}";
            public const string DiscordHttpError = "[DiscordNotificationProvider] Discord devolvió un código de error HTTP {StatusCode} al enviar la notificación '{Title}': {ResponseBody}";
        }
    }

    public static class Discord
    {
        public const string DefaultFooter = "BanchoMultiplayerBot • Sistema de Monitoreo y Notificaciones";

        public static class Labels
        {
            public const string BotUsername = "Usuario IRC";
            public const string PpService = "Servicio de PP";
            public const string ConfiguredLobbies = "Lobbies Configurados";
            public const string ActiveLobbies = "Lobbies Activos";
            public const string LobbyDetails = "Detalle de Salas";
            public const string ReconnectDelay = "Espera Inicial";
            public const string MaxAttempts = "Intentos Máximos";
            public const string Attempt = "Intento Exitoso";
            public const string DisconnectedTime = "Tiempo Desconectado";
            public const string ActionTaken = "Acción Tomada";
            public const string LobbyId = "ID de Lobby";
            public const string LobbyName = "Sala";
            public const string Channel = "Canal IRC";
            public const string MatchHistory = "Historial de Partida";
            public const string GameMode = "Modo / Capacidad";
            public const string LastEvent = "Último Evento";
            public const string InactiveTime = "Tiempo Inactivo";
            public const string PlayersFinished = "Jugadores Finalizados";
            public const string Beatmap = "Mapa";
            public const string SanctionedPlayer = "Jugador Sancionado";
            public const string Administrator = "Administrador";
            public const string BanType = "Tipo de Sanción";
            public const string Duration = "Duración";
            public const string Expiration = "Expiración (UTC)";
            public const string Reason = "Motivo";
            public const string BeatmapSetId = "BeatmapSet";
            public const string JoinAttempts = "Intentos Recientes";
            public const string Host = "Host";
            public const string ActiveMods = "Mods Activos";
            public const string StarRating = "Dificultad (SR)";
            public const string AllowedRange = "Rango Permitido";
            public const string Violations = "Violaciones Acumuladas";
            public const string OsuUser = "Usuario osu!";
            public const string OsuUserId = "ID de Usuario";
            public const string LeaderboardRank = "Puesto Global";
            public const string ScoreDetails = " Estadísticas del Puntaje";
        }

#if !INTERNAL_BOT_MESSAGES
        public static NotificationMessage BotStarting(string username, bool ppAvailable, string? ppUrl) => new()
        {
            Title = "🚀 Bot Iniciando",
            Description = "El bot está iniciando sus servicios internos y preparándose para conectar con el servidor IRC de Bancho.",
            Severity = NotificationSeverity.Info,
            Fields =
            [
                new NotificationField(Labels.BotUsername, $"`{username}`"),
                new NotificationField(Labels.PpService, ppAvailable ? $"🟢 Configurado (`{ppUrl}`)" : "⚪ Desactivado")
            ]
        };

        public static NotificationMessage BotReady(string username, int activeCount, int totalCount, string lobbiesSummary) => new()
        {
            Title = "✅ Bot Conectado y Listo",
            Description = "La conexión con Bancho fue establecida y el proceso de inicialización de salas ha finalizado.",
            Severity = activeCount == totalCount ? NotificationSeverity.Success : NotificationSeverity.Warning,
            Fields =
            [
                new NotificationField(Labels.BotUsername, $"`{username}`"),
                new NotificationField(Labels.ActiveLobbies, $"`{activeCount} / {totalCount}`"),
                new NotificationField(Labels.LobbyDetails, string.IsNullOrWhiteSpace(lobbiesSummary) ? "Sin lobbies configurados" : lobbiesSummary, Inline: false)
            ]
        };

        public static NotificationMessage BotShuttingDown(int lobbyCount) => new()
        {
            Title = "🛑 Bot Apagándose",
            Description = "Se inició el apagado controlado del bot. Cerrando instancias de salas y desconectando de Bancho.",
            Severity = NotificationSeverity.Info,
            Fields =
            [
                new NotificationField(Labels.ConfiguredLobbies, $"`{lobbyCount}` sala(s)")
            ]
        };

        public static NotificationMessage BanchoConnectionLost(string username, int lobbyCount, int reconnectDelaySeconds, int maxAttempts) => new()
        {
            Title = "⚠️ Conexión Perdida con Bancho",
            Description = "Se interrumpió la conexión IRC con Bancho. El bot intentará reconectarse automáticamente.",
            Severity = NotificationSeverity.Error,
            Fields =
            [
                new NotificationField(Labels.BotUsername, $"`{username}`"),
                new NotificationField(Labels.ConfiguredLobbies, $"`{lobbyCount}` sala(s) afectada(s)"),
                new NotificationField(Labels.ReconnectDelay, $"`{reconnectDelaySeconds}s` ({maxAttempts} intentos máx.)")
            ]
        };

        public static NotificationMessage BanchoReconnected(string username, int attempt, int maxAttempts) => new()
        {
            Title = "🔄 Reconexión Exitosa con Bancho",
            Description = "El bot recuperó la conexión con el servidor IRC de Bancho y está reanudando las salas.",
            Severity = NotificationSeverity.Success,
            Fields =
            [
                new NotificationField(Labels.BotUsername, $"`{username}`"),
                new NotificationField(Labels.Attempt, $"`{attempt} / {maxAttempts}`")
            ]
        };

        public static NotificationMessage BanchoReconnectExhausted(string username, int maxAttempts, int attemptDelaySeconds) => new()
        {
            Title = "🚨 Fallo Crítico: Reconexión a Bancho Agotada",
            Description = "El bot no pudo restablecer la conexión con Bancho tras agotar todos los intentos configurados.",
            Severity = NotificationSeverity.Critical,
            Fields =
            [
                new NotificationField(Labels.BotUsername, $"`{username}`"),
                new NotificationField(Labels.MaxAttempts, $"`{maxAttempts}` intentos (cada `{attemptDelaySeconds}s`)"),
                new NotificationField(Labels.ActionTaken, "Cierre del cliente IRC")
            ]
        };

        public static NotificationMessage WatchdogEmergencyShutdown(int disconnectedSeconds) => new()
        {
            Title = "🚨 Apagado de Emergencia (Watchdog)",
            Description = "El servicio de salud detectó que el bot permaneció desconectado de Bancho más tiempo del permitido.",
            Severity = NotificationSeverity.Critical,
            Fields =
            [
                new NotificationField(Labels.DisconnectedTime, $"`> {disconnectedSeconds} segundos`"),
                new NotificationField(Labels.ActionTaken, "Detención del proceso para forzar un reinicio en frío")
            ]
        };

        public static NotificationMessage BanchoMaintenanceRestart(string activeLobbiesList) => new()
        {
            Title = "🛠️ Mantenimiento de Bancho Detectado",
            Description = "Bancho anunció un reinicio por mantenimiento en 1 minuto. Las salas serán recreadas automáticamente cuando el servidor vuelva a estar en línea.",
            Severity = NotificationSeverity.Warning,
            DeduplicationKey = "bancho-maintenance-restart",
            DeduplicationWindow = TimeSpan.FromMinutes(5),
            Fields =
            [
                new NotificationField(Labels.LobbyDetails, string.IsNullOrWhiteSpace(activeLobbiesList) ? "N/A" : activeLobbiesList, Inline: false)
            ]
        };

        public static NotificationMessage LobbyChannelCreated(int lobbyId, string lobbyName, string channelName, string mode, int size)
        {
            var matchId = channelName.StartsWith("#mp_") ? channelName[4..] : channelName;
            var matchUrl = $"https://osu.ppy.sh/community/matches/{matchId}";
            return new NotificationMessage
            {
                Title = "🆕 Nueva Sala Multijugador Creada",
                Description = $"Se creó e inicializó un nuevo canal multijugador en Bancho para la sala **{lobbyName}**.",
                Severity = NotificationSeverity.Success,
                Url = matchUrl,
                Fields =
                [
                    new NotificationField(Labels.LobbyId, $"`#{lobbyId}`"),
                    new NotificationField(Labels.Channel, $"`{channelName}`"),
                    new NotificationField(Labels.GameMode, $"`{mode}` • `{size}` slots"),
                    new NotificationField(Labels.MatchHistory, $"[Ver historial en osu!]({matchUrl})", Inline: false)
                ]
            };
        }

        public static NotificationMessage LobbyCreationTimeout(int lobbyId, string lobbyName) => new()
        {
            Title = "❌ Fallo al Inicializar Sala Multijugador",
            Description = $"La sala **{lobbyName}** no respondió ni logró estar lista dentro del tiempo límite de 10 segundos.",
            Severity = NotificationSeverity.Error,
            Fields =
            [
                new NotificationField(Labels.LobbyId, $"`#{lobbyId}`"),
                new NotificationField(Labels.LobbyName, lobbyName),
                new NotificationField(Labels.ActionTaken, "Estado marcado como `ChannelCreationFailed`")
            ]
        };

        public static NotificationMessage LobbyEventTimeout(int lobbyId, string lobbyName, string channelName, DateTime lastEventTimeUtc)
        {
            var inactiveDuration = DateTime.UtcNow - lastEventTimeUtc;
            return new NotificationMessage
            {
                Title = "⏳ Sala Inactiva Detectada (Recreando)",
                Description = $"No se registraron eventos en la sala **{lobbyName}** durante más de 2 horas. Se asumió que el canal murió y se inició su recreación automática.",
                Severity = NotificationSeverity.Warning,
                Fields =
                [
                    new NotificationField(Labels.LobbyId, $"`#{lobbyId}`"),
                    new NotificationField(Labels.Channel, $"`{channelName}`"),
                    new NotificationField(Labels.InactiveTime, $"`{inactiveDuration.TotalHours:0.0} horas`"),
                    new NotificationField(Labels.LastEvent, $"`{lastEventTimeUtc:yyyy-MM-dd HH:mm:ss} UTC`")
                ]
            };
        }

        public static NotificationMessage MatchStuckAborted(int lobbyId, string channelName, int finishedPlayers, int totalPlayers, string beatmapTitle, int beatmapId) => new()
        {
            Title = "⚠️ Partida Atascada Abortada",
            Description = "Más de la mitad de los jugadores terminaron el mapa hace más de 30 segundos pero la partida seguía en curso. Se ejecutó `!mp abort` automáticamente.",
            Severity = NotificationSeverity.Warning,
            Fields =
            [
                new NotificationField(Labels.LobbyId, $"`#{lobbyId}` (`{channelName}`)"),
                new NotificationField(Labels.PlayersFinished, $"`{finishedPlayers} / {totalPlayers}`"),
                new NotificationField(Labels.Beatmap, beatmapId > 0 ? $"[{beatmapTitle}](https://osu.ppy.sh/b/{beatmapId}) (`#{beatmapId}`)" : beatmapTitle, Inline: false)
            ]
        };

        public static NotificationMessage PlayerBanned(
            int lobbyId,
            string channelName,
            string targetPlayer,
            string adminName,
            bool isHostBan,
            int lengthDays,
            DateTime expireUtc,
            string reason) => new()
        {
            Title = "🔨 Jugador Sancionado",
            Description = isHostBan
                ? $"Se aplicó una **restricción de Host** al jugador **{targetPlayer}**."
                : $"El jugador **{targetPlayer}** fue **baneado y expulsado** de las salas del bot.",
            Severity = NotificationSeverity.Moderation,
            Fields =
            [
                new NotificationField(Labels.SanctionedPlayer, $"[{targetPlayer}](https://osu.ppy.sh/users/@{ Uri.EscapeDataString(targetPlayer) })"),
                new NotificationField(Labels.Administrator, $"`{adminName}`"),
                new NotificationField(Labels.BanType, isHostBan ? "`Host Ban (!ban)`" : "`Player Ban (!pban)`"),
                new NotificationField(Labels.Duration, $"`{lengthDays}` día(s)"),
                new NotificationField(Labels.Expiration, $"`{expireUtc:yyyy-MM-dd HH:mm} UTC`"),
                new NotificationField(Labels.LobbyId, $"`#{lobbyId}` (`{channelName}`)"),
                new NotificationField(Labels.Reason, reason, Inline: false)
            ]
        };

        public static NotificationMessage MapsetBanned(int lobbyId, string channelName, int mapSetId, string adminName) => new()
        {
            Title = "🚫 Mapset Baneado",
            Description = $"El mapset **#{mapSetId}** fue agregado a la lista negra de mapas prohibidos.",
            Severity = NotificationSeverity.Moderation,
            Url = $"https://osu.ppy.sh/beatmapsets/{mapSetId}",
            Fields =
            [
                new NotificationField(Labels.BeatmapSetId, $"[#{mapSetId}](https://osu.ppy.sh/beatmapsets/{mapSetId})"),
                new NotificationField(Labels.Administrator, $"`{adminName}`"),
                new NotificationField(Labels.LobbyId, $"`#{lobbyId}` (`{channelName}`)")
            ]
        };

        public static NotificationMessage AdminPromoted(string targetPlayer, string adminName) => new()
        {
            Title = "👑 Administrador Asignado",
            Description = $"El usuario **{targetPlayer}** ha recibido permisos de Administrador.",
            Severity = NotificationSeverity.Info,
            Fields =
            [
                new NotificationField("Nuevo Administrador", $"[{targetPlayer}](https://osu.ppy.sh/users/@{Uri.EscapeDataString(targetPlayer)})"),
                new NotificationField(Labels.Administrator, $"`{adminName}`")
            ]
        };

        public static NotificationMessage AdminDemoted(string targetPlayer, string adminName) => new()
        {
            Title = "⚠️ Permisos de Administrador Revocados",
            Description = $"Se han revocado los permisos de Administrador al usuario **{targetPlayer}**.",
            Severity = NotificationSeverity.Warning,
            Fields =
            [
                new NotificationField("Usuario Afectado", $"[{targetPlayer}](https://osu.ppy.sh/users/@{Uri.EscapeDataString(targetPlayer)})"),
                new NotificationField(Labels.Administrator, $"`{adminName}`")
            ]
        };

        public static NotificationMessage BannedPlayerSpamAutoBan(int lobbyId, string channelName, string playerName, int attempts) => new()
        {
            Title = "🛡️ Usuario Sancionado Bloqueado permanentemente de la Sala",
            Description = $"El usuario sancionado **{playerName}** intentó reingresar repetidamente a la sala en un corto período, por lo que se aplicó `!mp ban` directo en el canal.",
            Severity = NotificationSeverity.Moderation,
            Fields =
            [
                new NotificationField(Labels.SanctionedPlayer, $"[{playerName}](https://osu.ppy.sh/users/@{Uri.EscapeDataString(playerName)})"),
                new NotificationField(Labels.JoinAttempts, $"`{attempts}` ingresos en <15 min"),
                new NotificationField(Labels.LobbyId, $"`#{lobbyId}` (`{channelName}`)")
            ]
        };

        public static NotificationMessage InvalidMapMatchAborted(
            int lobbyId,
            string channelName,
            string hostName,
            int beatmapId,
            string beatmapTitle,
            string activeMods,
            double calculatedSr,
            float minSr,
            float maxSr) => new()
        {
            Title = "🛑 Partida Abortada por Violación de Reglamento al Iniciar",
            Description = "Se detectó un intento de iniciar la partida con un mapa o combinación de mods fuera de los límites permitidos de la sala.",
            Severity = NotificationSeverity.Error,
            Fields =
            [
                new NotificationField(Labels.LobbyId, $"`#{lobbyId}` (`{channelName}`)"),
                new NotificationField(Labels.Host, $"`{hostName}`"),
                new NotificationField(Labels.ActiveMods, string.IsNullOrWhiteSpace(activeMods) ? "`None`" : $"`{activeMods}`"),
                new NotificationField(Labels.StarRating, $"`{calculatedSr:0.00}*`"),
                new NotificationField(Labels.AllowedRange, $"`{minSr:0.0#}* - {maxSr:0.0#}*`"),
                new NotificationField(Labels.Beatmap, $"[{beatmapTitle}](https://osu.ppy.sh/b/{beatmapId}) (`#{beatmapId}`)", Inline: false)
            ]
        };

        public static NotificationMessage HostSkippedForViolations(int lobbyId, string channelName, string hostName, int maxViolations) => new()
        {
            Title = "⏭️ Host Saltado Automáticamente por Violaciones",
            Description = $"El anfitrión **{hostName}** superó el límite permitido de selecciones inválidas de mapa y fue saltado en la cola.",
            Severity = NotificationSeverity.Warning,
            Fields =
            [
                new NotificationField(Labels.LobbyId, $"`#{lobbyId}` (`{channelName}`)"),
                new NotificationField(Labels.Host, $"`{hostName}`"),
                new NotificationField(Labels.Violations, $"`{maxViolations}` selecciones inválidas consecutivas")
            ]
        };

        public static NotificationMessage HostKickedForAfk(int lobbyId, string channelName, string hostName) => new()
        {
            Title = "💤 Host Expulsado por Inactividad (AFK)",
            Description = $"El anfitrión **{hostName}** fue detectado como AFK mediante `!stat` y fue removido de la sala para no bloquear la rotación.",
            Severity = NotificationSeverity.Warning,
            Fields =
            [
                new NotificationField(Labels.LobbyId, $"`#{lobbyId}` (`{channelName}`)"),
                new NotificationField(Labels.Host, $"`{hostName}`")
            ]
        };

        public static NotificationMessage WebAuthAttempt(string username, string userId, bool isSuccess) => new()
        {
            Title = isSuccess ? "🔐 Acceso de Administrador al Panel Web" : "🚨 Intento de Acceso No Autorizado al Panel Web",
            Description = isSuccess
                ? $"El usuario **{username}** inició sesión correctamente en el panel de administración vía osu! OAuth."
                : $"El usuario **{username}** intentó autenticarse en el panel web pero fue rechazado por no poseer rol de administrador.",
            Severity = isSuccess ? NotificationSeverity.Info : NotificationSeverity.Moderation,
            Fields =
            [
                new NotificationField(Labels.OsuUser, $"[{username}](https://osu.ppy.sh/users/{userId})"),
                new NotificationField(Labels.OsuUserId, $"`{userId}`")
            ]
        };

        public static NotificationMessage LeaderboardScoreAchieved(
            int lobbyId,
            string channelName,
            string playerName,
            int placement,
            int beatmapId,
            int beatmapSetId,
            string beatmapTitle,
            string scoreDetails) => new()
        {
            Title = $"🏆 ¡Nuevo Top #{placement} en el Leaderboard Global!",
            Description = $"El jugador **{playerName}** acaba de lograr el puesto **#{placement}** en el leaderboard global del mapa jugando en el lobby.",
            Severity = NotificationSeverity.Gameplay,
            Url = $"https://osu.ppy.sh/b/{beatmapId}",
            ThumbnailUrl = beatmapSetId > 0 ? $"https://b.ppy.sh/thumb/{beatmapSetId}l.jpg" : null,
            Fields =
            [
                new NotificationField(Labels.OsuUser, $"[{playerName}](https://osu.ppy.sh/users/@{Uri.EscapeDataString(playerName)})"),
                new NotificationField(Labels.LeaderboardRank, $"`#{placement}`"),
                new NotificationField(Labels.LobbyId, $"`#{lobbyId}` (`{channelName}`)"),
                new NotificationField(Labels.Beatmap, $"[{beatmapTitle}](https://osu.ppy.sh/b/{beatmapId})", Inline: false),
                new NotificationField(Labels.ScoreDetails, scoreDetails, Inline: false)
            ]
        };
#endif
    }

    public static class Chat
    {
        public static class Usage
        {
            public const string AddAdmin = "!addadmin <player_name>";
            public const string RemoveAdmin = "!removeadmin <player_name>";
            public const string AutoSkip = "!autoskip <on|off>";
            public const string Ban = "!ban <player_name> <days> [reason]";
            public const string PlayerBan = "!pban <player_name> <days> [reason]";
            public const string BanMapset = "!banmapset <mapset_id>";
            public const string Restart = "!restart <lobby_id>";
            public const string SetHost = "!sethost <player_name>";
            public const string SetQueuePosition = "!setqueuepos <player_name> <position>";
            public const string Start = "!start <seconds>";
            public const string TeamsMode = "!teamsmode <on/off>";

            public static string Format(string? usage) => $"Usage: {usage}";
        }

        public static class Admin
        {
            public const string CannotDemoteOwner = "Cannot remove administrator privileges from the bot owner.";
            public const string CannotDemoteSelf = "You cannot remove your own administrator privileges.";

            public static string PlayerPromoted(string targetPlayer, string adminPlayer) =>
                $"Player {targetPlayer} has been granted administrator permissions by {adminPlayer}.";

            public static string PlayerDemoted(string targetPlayer, string adminPlayer) =>
                $"Player {targetPlayer} is no longer an administrator (revoked by {adminPlayer}).";

            public static string AlreadyAdministrator(string targetPlayer) =>
                $"Player {targetPlayer} is already an administrator.";

            public static string NotAdministrator(string targetPlayer) =>
                $"Player {targetPlayer} is not an administrator.";

            public static string AdminList(IEnumerable<string> admins) =>
                $"Current administrators: {string.Join(", ", admins)}";
        }

        public static class Votes
        {
            public const string AbortDescription = "Abort the match";
            public const string SkipDescription = "Skip the host";
            public const string StartDescription = "Start the match";

            public static string Passed(string description, int currentVotes, int requiredVotes) =>
                $"{description} vote passed! ({currentVotes}/{requiredVotes})";

            public static string Progress(string description, int currentVotes, int requiredVotes) =>
                $"{description} vote ({currentVotes}/{requiredVotes})";
        }

        public static class Announcements
        {
            public static string Notice(string message, string spamFilter) =>
                $"Notice: {message} {spamFilter}";

            public static string BanchoMaintenanceLine1(string spamFilter) =>
                $"Bancho is about to restart for maintenance. This lobby will be automatically recreated a few minutes after Bancho is back online. {spamFilter}";

            public static string BanchoMaintenanceLine2(string spamFilter) =>
                $"Please search for the lobby name if you don't see it in the list right away. Thanks for playing! {spamFilter}";
        }

        public static class AutoStart
        {
            public const string StartingIn10Seconds = "Starting match in 10 seconds, use !stop to abort";

            public static string QueuedToStart(int seconds) =>
                $"Queued to start match in {seconds} seconds, use !stop to abort";
        }

        public static class Bans
        {
            public const string InvalidMapsetId = "Invalid mapset ID.";
            public const string UserNotFound = "User not found.";
            public const string InvalidBanLength = "Invalid ban length (must be a number of days).";

            public static string MapsetBanned(int mapSetId) =>
                $"Beatmapset #{mapSetId} has been banned successfully.";

            public static string PlayerBanned(string playerName, bool hostBan, int days, string reason) =>
                hostBan
                    ? $"Player {playerName} has been restricted from hosting for {days} day(s) (Reason: {reason})."
                    : $"Player {playerName} has been banned from the lobby for {days} day(s) (Reason: {reason}).";
        }

        public static class DynamicRoom
        {
            public static string RoomNameFormat(float minSr, float maxSr) =>
                $"{minSr:.0#}* - {maxSr:.0#}* Dynamic SR | Auto Host Rotate";

            public static string StarRatingLimitUpdated(float minSr, float maxSr) =>
                $"The star rating limit has now been updated to {minSr:.0#}* - {maxSr:.0#}*!";
        }

        public static class HostQueue
        {
            public const string PlayerNotFoundInQueue = "Couldn't find player in queue.";
            public const string PlayerNotFoundInLobby = "Player not found in lobby.";
            public const string InvalidPositionArgument = "Invalid position argument.";
            public const string QueuePositionOutOfBounds = "Queue position out of bounds.";

            public static string QueueList(string queueStr) => $"Queue: {queueStr}";

            public static string QueuePosition(string playerName, int position) =>
                $"Queue position for {playerName}: #{position}";

            public static string AutoSkipStatus(string playerName, bool enabled) =>
                $"{playerName}, your auto-skip is currently {(enabled ? "enabled" : "disabled")}.";

            public static string AutoSkipUpdated(string playerName, bool enabled) =>
                $"{playerName}, your auto-skip is now {(enabled ? "enabled" : "disabled")}.";
        }

        public static class MapManager
        {
            public const string BeatmapNotSubmitted = "The selected beatmap is not submitted, please pick another one.";
            public const string OsuApiBeatmapError = "osu! API error while trying to fetch beatmap information.";
            public const string OsuApiTimeout = "osu! API timed out while trying to fetch beatmap information.";
            public const string InternalBeatmapError = "Internal error while trying to fetch beatmap information.";
            public const string BeatmapRemoved = "The selected beatmap has been removed from the osu! website.";
            public const string InvalidPlayingMapAborted = "Detected an attempt to play a map outside of the lobby's star rating limits! Aborting match...";

            public static string MatchFinishedNotification(int channelId, string lobbyName) =>
                $"Match has finished in the [osu://mp/{channelId} {lobbyName}] lobby!";

            public static string BeatmapTooLong(TimeSpan maxLength) =>
                $"The selected beatmap is too long. Max map length: {maxLength:mm\\:ss}.";

            public static string BeatmapTooShort(TimeSpan minLength) =>
                $"The selected beatmap is too short. Min map length: {minLength:mm\\:ss}.";

            public static string StarRatingTooHigh(double mapSr, float maxSr) =>
                $"The selected beatmap's star rating is too high for this lobby ({mapSr:0.00}* > {maxSr:0.0}*).";

            public static string StarRatingTooLow(float minSr, double mapSr) =>
                $"The selected beatmap's star rating is too low for this lobby ({minSr:0.0}* > {mapSr:0.00}*).";

            public static string InvalidGameMode(string modeName) =>
                $"Please only pick beatmaps for the {modeName} game mode.";

            public static string BeatmapBanned(string? title) =>
                title != null
                    ? $"The selected beatmap ({title}) is banned in this lobby."
                    : "The selected beatmap is banned in this lobby.";

            public static string Regulations(float minSr, float maxSr, TimeSpan maxLength, string requiredModeName) =>
                $"Star rating: {minSr:.0#}* - {maxSr:.0#}* | Max length: {maxLength:mm\\:ss} | Mode: {requiredModeName}";

            public static string Mirrors(int setId) =>
                $"[https://beatconnect.io/b/{setId} BeatConnect Mirror] - [https://osu.direct/d/{setId} osu.direct Mirror] - [https://catboy.best/d/{setId} Mino Mirror]";

            public static string TimeLeft(string timeLeft, bool pingEnabled) =>
                pingEnabled
                    ? $"Estimated time left of current map: {timeLeft}, you will be notified when the map is finished."
                    : $"Estimated time left of current map: {timeLeft}";

            public static string BeatmapHeaderWithMirrors(int beatmapId, int setId, string? artist, string? title, string version) =>
                $"[https://osu.ppy.sh/b/{beatmapId} {artist} - {title} [{version}]] - ({Mirrors(setId)})";

            public static string BeatmapStatsSummary(double starRatingRounded, string status, TimeSpan length, float bpm) =>
                $"(Star Rating: {starRatingRounded:.0#} | {status} | Length: {length:mm\\:ss} | BPM: {bpm})";

            public static string BeatmapDifficultyWithPp(float ar, float cs, float od, float hp, int pp100, int pp98, int pp95) =>
                $"(AR: {ar} | CS: {cs} | OD: {od} | HP: {hp} | 100%: {pp100}pp | 98%: {pp98}pp | 95%: {pp95}pp)";

            public static string BeatmapDifficultyWithoutPp(float ar, float cs, float od, float hp) =>
                $"(AR: {ar} | CS: {cs} | OD: {od} | HP: {hp})";

            public static string SkippingHostForViolations(int maxViolations) =>
                $"Skipping host automatically due to {maxViolations} consecutive map violations!";
        }

        public static class RoomManager
        {
            public const string StuckMatchAborting = "Detected a possibly stuck match, aborting...";
        }

        public static class FunCommands
        {
            public const string MapNotPlayedYet = "This map has not been played in this lobby yet!";
            public const string NoScoresOnMapYet = "No scores have been set on this map in this lobby yet!";
            public const string PpCalculatorUnavailable = "Performance point calculator is not available.";
            public const string PpCalculationError = "Error calculating performance points.";
            public const string OsuApiRequestFailed = "osu! API request failed.";
            public const string BeatmapTagsMissing = "Beatmap tags are missing.";
            public const string NoBeatmapTagsFound = "No beatmap tags found.";

            public static string PlayTime(string? playerName, string currentPlaytime, string matchesQuantity, string totalPlaytime, double totalHours) =>
                $"{playerName} has been here for {currentPlaytime}, playing {matchesQuantity}. ({totalPlaytime} ({totalHours:F0}h) in total).";

            public static string PlayStatistics(string? playerName, string matchesQuantity, int numberOneResults) =>
                $"{playerName} has played {matchesQuantity} with a total of {numberOneResults} #1's.";

            public static string PersonalMapStatsMostlyFail(string? playerName, string timesPlayedQuantity) =>
                $"{playerName}, you've played this map {timesPlayedQuantity} in this lobby! You usually fail the map! :(";

            public static string PersonalMapStatsMostlyPass(string? playerName, string timesPlayedQuantity, string mostCommonRank, double avgAccuracy) =>
                $"{playerName}, you've played this map {timesPlayedQuantity} in this lobby! You usually pass the map with an {mostCommonRank} rank, average accuracy: {avgAccuracy:0.00}%!";

            public static string PersonalMapStatsNeverPlayed(string? playerName) =>
                $"{playerName}, you haven't played this map in this lobby yet!";

            public static string BestMapStats(string? playerName, string? bestScoreString) =>
                bestScoreString != null
                    ? $"{playerName}, your best score on this map in this lobby is {bestScoreString}"
                    : PersonalMapStatsNeverPlayed(playerName);

            public static string LastPlayed(string humanizedTime) =>
                $"The map was last played {humanizedTime}!";

            public static string MapRecord(string ircUserName, string userName, string scoreString) =>
                $"[https://osu.ppy.sh/users/@{ircUserName} {userName}] has the best lobby score on this map with {scoreString}";

            public static string PerformancePoints(string sender, int pp100, int pp98, int pp95) =>
                $"{sender}, 100%: {pp100}pp | 98%: {pp98}pp | 95%: {pp95}pp";

            public static string LeaveCount(int leftCount) =>
                $"There were {leftCount} player(s) that left the lobby on the previous map!";

            public static string BeatmapTags(IEnumerable<string> tags) =>
                $"Beatmap tags are: {string.Join(", ", tags)}";

            public static string TeamsModeUpdated(bool enabled) =>
                $"Teams mode has been {(enabled ? "enabled" : "disabled")}.";

            public static string LeaderboardCongratulation(string playerName, int position) =>
                $"Congratulations {playerName} for getting #{position} on the map's global leaderboard!";
        }

        public static class Commands
        {
            public const string HelpResponse = "osu! auto host rotation bot (v2.0.0) [https://github.com/matte-ek/BanchoMultiplayerBot/blob/master/COMMANDS.md Help & Commands]";
            public const string RecentScoreUserNotFound = "Unable to find user.";
            public const string RecentScoreNotFound = "Unable to find any recent score.";
            public const string RecentScoreBeatmapNotFound = "Unable to find beatmap.";
            public const string RecentScorePpFailed = "Unable to calculate performance points.";
            public const string RestartInProgress = "Restart is currently in progress.";
            public const string RestartingLobby = "Restarting lobby, this might take a few seconds...";

            public static string MatchLink(string? matchId) =>
                $"Match history available [https://osu.ppy.sh/community/matches/{matchId} here.]";

            public static string Version(string commitHash, string commitHashSubset) =>
                $"BanchoMultiplayerBot@[https://github.com/matte-ek/BanchoMultiplayerBot/commit/{commitHash} {commitHashSubset}]";
        }
    }
}
