namespace Clay.Types.Element;

internal enum ClayPointerCaptureMode //TODO NOTE: Once this is implemented, this can cause some damage because Passthrough will be increased by 1...
{
    CLAY_POINTER_CAPTURE_MODE_CAPTURE,

    //CLAY_POINTER_CAPTURE_MODE_PARENT, TODO not yet implemented in clay.h
    CLAY_POINTER_CAPTURE_MODE_PASSTHROUGH,
}