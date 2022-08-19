namespace QuotexAPI
{
    enum SocketState
    {
        Idle = 0,
        OpenOrderWait = 1,
        CancelOrderWait = 2,
        OpenPndingOrderWait = 3,
        CancelPendingOrderWait = 4
    }
}
