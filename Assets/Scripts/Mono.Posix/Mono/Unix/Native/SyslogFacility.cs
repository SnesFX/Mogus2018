using System;

namespace Mono.Unix.Native
{
	[Map]
	[CLSCompliant(false)]
	public enum SyslogFacility
	{
		LOG_KERN = 0,
		LOG_USER = 8,
		LOG_MAIL = 16,
		LOG_DAEMON = 24,
		LOG_AUTH = 32,
		LOG_SYSLOG = 40,
		LOG_LPR = 48,
		LOG_NEWS = 56,
		LOG_UUCP = 64,
		LOG_CRON = 72,
		LOG_AUTHPRIV = 80,
		LOG_FTP = 88,
		LOG_LOCAL0 = 128,
		LOG_LOCAL1 = 136,
		LOG_LOCAL2 = 144,
		LOG_LOCAL3 = 152,
		LOG_LOCAL4 = 160,
		LOG_LOCAL5 = 168,
		LOG_LOCAL6 = 176,
		LOG_LOCAL7 = 184
	}
}
