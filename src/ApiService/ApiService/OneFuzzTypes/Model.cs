using Microsoft.OneFuzz.Service.OneFuzzLib.Orm;
using System;
using System.Collections.Generic;
using PoolName = System.String;
using Region = System.String;
using Container = System.String;

namespace Microsoft.OneFuzz.Service;


/// Convention for database entities:
/// All entities are represented by immutable records
/// All database entities need to derive from EntityBase
/// Only properties that also apears as parameter initializers are mapped to the database
/// The name of the property will be tranlated to snake case and used as the column name
/// It is possible to rename the column name by using the [property:JsonPropertyName("column_name")] attribute
/// the "partion key" and "row key" are identified by the [PartitionKey] and [RowKey] attributes
/// Guids are mapped to string in the db

public record Authentication
(
    string Password,
    string PublicKey,
    string PrivateKey
);

[SkipRename]
public enum HeartbeatType
{
    MachineAlive,
    TaskAlive,
}

public record HeartbeatData(HeartbeatType Type);

public record TaskHeartbeatEntry(
    Guid TaskId,
    Guid? JobId,
    Guid MachineId,
    HeartbeatData[] Data
    );
public record NodeHeartbeatEntry(Guid NodeId, HeartbeatData[] Data);

public record NodeCommandStopIfFree();

public record StopNodeCommand();

public record StopTaskNodeCommand(Guid TaskId);

public record NodeCommandAddSshKey(string PublicKey);


public record NodeCommand
(
    StopNodeCommand? Stop,
    StopTaskNodeCommand? StopTask,
    NodeCommandAddSshKey? AddSshKey,
    NodeCommandStopIfFree? StopIfFree
);

public enum NodeTaskState
{
    Init,
    SettingUp,
    Running,
}

public record NodeTasks
(
    Guid MachineId,
    Guid TaskId,
    NodeTaskState State = NodeTaskState.Init
);

public enum NodeState
{
    Init,
    Free,
    SettingUp,
    Rebooting,
    Ready,
    Busy,
    Done,
    Shutdown,
    Halt,
}

public record ProxyHeartbeat
(
    Region Region,
    Guid ProxyId,
    List<ProxyForward> Forwards,
    DateTimeOffset TimeStamp
);

public partial record Node
(
    DateTimeOffset? InitializedAt,
    [PartitionKey] PoolName PoolName,
    Guid? PoolId,
    [RowKey] Guid MachineId,
    NodeState State,
    Guid? ScalesetId,
    DateTimeOffset Heartbeat,
    string Version,
    bool ReimageRequested,
    bool DeleteRequested,
    bool DebugKeepNode
) : EntityBase();


public partial record ProxyForward
(
    [PartitionKey] Region Region,
    [RowKey] int DstPort,
    int SrcPort,
    string DstIp
) : EntityBase();

public partial record ProxyConfig
(
    Uri Url,
    string Notification,
    Region Region,
    Guid? ProxyId,
    List<ProxyForward> Forwards,
    string InstanceTelemetryKey,
    string MicrosoftTelemetryKey

);

public partial record Proxy
(
    [PartitionKey] Region Region,
    [RowKey] Guid ProxyId,
    DateTimeOffset? CreatedTimestamp,
    VmState State,
    Authentication Auth,
    string? Ip,
    Error? Error,
    string Version,
    ProxyHeartbeat? heartbeat
) : EntityBase();

public record Error(ErrorCode Code, string[]? Errors = null);

public record UserInfo(Guid? ApplicationId, Guid? ObjectId, String? Upn);


public record EventMessage(
    Guid EventId,
    EventType EventType,
    BaseEvent Event,
    Guid InstanceId,
    String InstanceName
) : EntityBase();


public record TaskDetails(

    TaskType Type,
    int Duration,
    string? TargetExe,
    Dictionary<string, string>? TargetEnv,
    List<string>? TargetOptions,
    int? TargetWorkers,
    bool? TargetOptionsMerge,
    bool? CheckAsanLog,
    bool? CheckDebugger,
    int? CheckRetryCount,
    bool? CheckFuzzerHelp,
    bool? ExpectCrashOnFailure,
    bool? RenameOutput,
    string? SupervisorExe,
    Dictionary<string, string>? SupervisorEnv,
    List<string>? SupervisorOptions,
    string? SupervisorInputMarker,
    string? GeneratorExe,
    Dictionary<string, string>? GeneratorEnv,
    List<string>? GeneratorOptions,
    string? AnalyzerExe,
    Dictionary<string, string>? AnalyzerEnv,
    List<string> AnalyzerOptions,
    ContainerType? WaitForFiles,
    string? StatsFile,
    StatsFormat? StatsFormat,
    bool? RebootAfterSetup,
    int? TargetTimeout,
    int? EnsembleSyncDelay,
    bool? PreserveExistingOutputs,
    List<string>? ReportList,
    int? MinimizedStackDepth,
    string? CoverageFilter
);

public record TaskVm(
    Region Region,
    string Sku,
    string Image,
    int Count,
    bool SpotInstance,
    bool? RebootAfterSetup
);

public record TaskPool(
    int Count,
    PoolName PoolName
);

public record TaskContainers(
    ContainerType Type,
    Container Name
);
public record TaskConfig(
   Guid JobId,
   List<Guid>? PrereqTasks,
   TaskDetails Task,
   TaskVm? Vm,
   TaskPool? Pool,
   List<TaskContainers>? Containers,
   Dictionary<string, string>? Tags,
   List<TaskDebugFlag>? Debug,
   bool? Colocate
   );


public record TaskEventSummary(
    DateTimeOffset? Timestamp,
    string EventData,
    string EventType
    );


public record NodeAssignment(
    Guid NodeId,
    Guid? ScalesetId,
    NodeTaskState State
    );


public record Task(
    // Timestamp: Optional[datetime] = Field(alias="Timestamp")
    [PartitionKey] Guid JobId,
    [RowKey] Guid TaskId,
    TaskState State,
    Os Os,
    TaskConfig Config,
    Error? Error,
    Authentication? Auth,
    DateTimeOffset? Heartbeat,
    DateTimeOffset? EndTime,
    UserInfo? UserInfo) : EntityBase()
{
    List<TaskEventSummary> Events { get; set; } = new List<TaskEventSummary>();
    List<NodeAssignment> Nodes { get; set; } = new List<NodeAssignment>();

}