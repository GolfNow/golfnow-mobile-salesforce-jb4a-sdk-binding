using System;
using ObjCRuntime;

namespace JB4ASDK
{
	[Native]
	public enum PushOriginationState : ulong
	{
		Background = 0,
		Foreground,
		Unknown = 999
	}

	[Native]
	public enum ConfigureSDKWithAppIDError : ulong
	{
		NoError = 0,
		InvalidAppIDError,
		InvalidAccessTokenError,
		UnableToReadRandomError,
		DatabaseAccessError,
		UnableToKeyDatabaseError,
		CCKeyDerivationPBKDFError,
		CCSymmetricKeyWrapError,
		CCSymmetricKeyUnwrapError,
		KeyChainError,
		UnableToReadCertificateError,
		RunOnceSimultaneouslyError,
		RunOnceError,
		InvalidLocationAndProximityError,
		SimulatorBlobError,
		KeyChainInvalidError
	}

	[Native]
	public enum RequestPIRecommendationsError : ulong
	{
		NoError = 0,
		InvalidMidParameterError = 1024,
		InvalidRetailerParameterError,
		InvalidPageParameterError,
		InvalidCompletionHandlerError
	}

	[Native]
	public enum MobilePushMessageType : ulong
	{
		Unknown,
		Basic,
		FenceEntry = 3,
		FenceExit,
		Proximity
	}

	[Native]
	public enum MobilePushGeofenceType : ulong
	{
		None = 0,
		Circle,
		Proximity = 3
	}

	[Native]
	public enum ETRegionRequestType : ulong
	{
		Unknown,
		Geofence,
		Proximity
	}

    [Native]
    public enum MobilePushContentType : ulong
    {
        None = 0,
        AlertMessage = 1 << 0,
        Page = 1 << 1,
        Ecp = 0x80000000 // 1 << 31
	}

	[Native]
	public enum MobilePushMessageFrequencyUnit : ulong
	{
		None,
		Year,
		Month,
		Week,
		Day,
		Hour
	}

	[Native]
	public enum LocationUpdateAppState : long
	{
		Background,
		Foreground
	}

	[Native]
	public enum GenericUpdateSendMethod : long
	{
		Get,
		Post,
		Put,
		Delete
	}

	[Native]
	public enum MPMessageSource : long
	{
		Database,
		Remote
	}
}
