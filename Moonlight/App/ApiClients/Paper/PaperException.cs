﻿namespace Moonlight.App.ApiClients.Paper;

public class PaperException : Exception
{
    public PaperException()
    {
    }

    public PaperException(string message) : base(message)
    {
    }

    public PaperException(string message, Exception inner) : base(message, inner)
    {
    }
}