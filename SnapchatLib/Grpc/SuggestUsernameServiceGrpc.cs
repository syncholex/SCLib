// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: SuggestUsernameService.proto
// </auto-generated>
#pragma warning disable 0414, 1591, 8981, 0612
#region Designer generated code

using grpc = global::Grpc.Core;

namespace SnapchatLibProto.Snapchat.Activation.Api {
  /// <summary>
  /// Define the service
  /// </summary>
  public static partial class SuggestUsernameService
  {
    static readonly string __ServiceName = "snapchat.activation.api.SuggestUsernameService";

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static void __Helper_SerializeMessage(global::Google.Protobuf.IMessage message, grpc::SerializationContext context)
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (message is global::Google.Protobuf.IBufferMessage)
      {
        context.SetPayloadLength(message.CalculateSize());
        global::Google.Protobuf.MessageExtensions.WriteTo(message, context.GetBufferWriter());
        context.Complete();
        return;
      }
      #endif
      context.Complete(global::Google.Protobuf.MessageExtensions.ToByteArray(message));
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static class __Helper_MessageCache<T>
    {
      public static readonly bool IsBufferMessage = global::System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(global::Google.Protobuf.IBufferMessage)).IsAssignableFrom(typeof(T));
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static T __Helper_DeserializeMessage<T>(grpc::DeserializationContext context, global::Google.Protobuf.MessageParser<T> parser) where T : global::Google.Protobuf.IMessage<T>
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (__Helper_MessageCache<T>.IsBufferMessage)
      {
        return parser.ParseFrom(context.PayloadAsReadOnlySequence());
      }
      #endif
      return parser.ParseFrom(context.PayloadAsNewBuffer());
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameRequestV2> __Marshaller_snapchat_activation_api_SCSuggestUsernamePbSuggestUsernameRequestV2 = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameRequestV2.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameResponseV2> __Marshaller_snapchat_activation_api_SCSuggestUsernamePbSuggestUsernameResponseV2 = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameResponseV2.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameRequestV2> __Marshaller_snapchat_activation_api_SCSuggestUsernamePbCheckUsernameRequestV2 = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameRequestV2.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameResponseV2> __Marshaller_snapchat_activation_api_SCSuggestUsernamePbCheckUsernameResponseV2 = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameResponseV2.Parser));

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameRequestV2, global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameResponseV2> __Method_SuggestUsername = new grpc::Method<global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameRequestV2, global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameResponseV2>(
        grpc::MethodType.Unary,
        __ServiceName,
        "SuggestUsername",
        __Marshaller_snapchat_activation_api_SCSuggestUsernamePbSuggestUsernameRequestV2,
        __Marshaller_snapchat_activation_api_SCSuggestUsernamePbSuggestUsernameResponseV2);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameRequestV2, global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameResponseV2> __Method_CheckUsername = new grpc::Method<global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameRequestV2, global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameResponseV2>(
        grpc::MethodType.Unary,
        __ServiceName,
        "CheckUsername",
        __Marshaller_snapchat_activation_api_SCSuggestUsernamePbCheckUsernameRequestV2,
        __Marshaller_snapchat_activation_api_SCSuggestUsernamePbCheckUsernameResponseV2);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::SnapchatLibProto.Snapchat.Activation.Api.SuggestUsernameServiceReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of SuggestUsernameService</summary>
    [grpc::BindServiceMethod(typeof(SuggestUsernameService), "BindService")]
    public abstract partial class SuggestUsernameServiceBase
    {
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameResponseV2> SuggestUsername(global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameRequestV2 request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameResponseV2> CheckUsername(global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameRequestV2 request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for SuggestUsernameService</summary>
    public partial class SuggestUsernameServiceClient : grpc::ClientBase<SuggestUsernameServiceClient>
    {
      /// <summary>Creates a new client for SuggestUsernameService</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public SuggestUsernameServiceClient(grpc::ChannelBase channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for SuggestUsernameService that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public SuggestUsernameServiceClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected SuggestUsernameServiceClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected SuggestUsernameServiceClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameResponseV2 SuggestUsername(global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameRequestV2 request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SuggestUsername(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameResponseV2 SuggestUsername(global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameRequestV2 request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_SuggestUsername, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameResponseV2> SuggestUsernameAsync(global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameRequestV2 request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SuggestUsernameAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameResponseV2> SuggestUsernameAsync(global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameRequestV2 request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_SuggestUsername, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameResponseV2 CheckUsername(global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameRequestV2 request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return CheckUsername(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameResponseV2 CheckUsername(global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameRequestV2 request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_CheckUsername, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameResponseV2> CheckUsernameAsync(global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameRequestV2 request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return CheckUsernameAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameResponseV2> CheckUsernameAsync(global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameRequestV2 request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_CheckUsername, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected override SuggestUsernameServiceClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new SuggestUsernameServiceClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static grpc::ServerServiceDefinition BindService(SuggestUsernameServiceBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_SuggestUsername, serviceImpl.SuggestUsername)
          .AddMethod(__Method_CheckUsername, serviceImpl.CheckUsername).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static void BindService(grpc::ServiceBinderBase serviceBinder, SuggestUsernameServiceBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_SuggestUsername, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameRequestV2, global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbSuggestUsernameResponseV2>(serviceImpl.SuggestUsername));
      serviceBinder.AddMethod(__Method_CheckUsername, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameRequestV2, global::SnapchatLibProto.Snapchat.Activation.Api.SCSuggestUsernamePbCheckUsernameResponseV2>(serviceImpl.CheckUsername));
    }

  }
}
#endregion
