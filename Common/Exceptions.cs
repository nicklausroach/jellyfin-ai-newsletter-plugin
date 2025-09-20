using System;

namespace Jellyfin.Plugin.AINewsletter.Common;

public class AINewsletterException : Exception
{
    public AINewsletterException(string message) : base(message)
    {
    }

    public AINewsletterException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class AIServiceException : AINewsletterException
{
    public AIServiceException(string message) : base(message)
    {
    }

    public AIServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class EmailServiceException : AINewsletterException
{
    public EmailServiceException(string message) : base(message)
    {
    }

    public EmailServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class TemplateException : AINewsletterException
{
    public TemplateException(string message) : base(message)
    {
    }

    public TemplateException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class ConfigurationException : AINewsletterException
{
    public ConfigurationException(string message) : base(message)
    {
    }

    public ConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}