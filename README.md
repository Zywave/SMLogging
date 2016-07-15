# SMLogging (Service Model Logging)

[![Build status](https://ci.appveyor.com/api/projects/status/jq63fmhc9xspiggk?svg=true)](https://ci.appveyor.com/project/JohnCruikshank/smlogging)

This library provides request and error logging behaviors for Windows Communication Foundation (WCF / Service Model) services.  While WCF has built-in logging features, this libary is intended to provide simple, consumable log files rather than the bulky traces produced by the built-in logging.  Think of it as IIS request logging for WCF services (including net.pipe and net.tcp traffic in addition to http).

## Log Format

### Request Log

LogDate (yyyy-MM-dd)
LogTime (HH:mm:ss.FFF) 
Disposition (Client|Dispatch)
MessageId (Guid)
RequestDate (yyyy-MM-dd)
RequestTime (HH:mm:ss.FFF) 
ClientIpAddress (IP Address)
ApplicationName (string)
MachingName (string)
ServerIpAddress (IP Address)
TargetScheme (URI Scheme)
TargetHost (URI Host)
TargetPort (URI Port)
Target (URI)
Action (URI)
Status (OneWay|Fault|Success)
FaultCode (string)
ResponseSize (int)
RequestSize (int)
TimeTaken (int)

### Error Log

LogDate (yyyy-MM-dd)
LogTime (HH:mm:ss.FFF) 
MessageId (Guid)
ClientIpAddress (IP Address)
ApplicationName (string)
MachingName (string)
MachineIpAddress (IP Address)
TargetScheme (URI Scheme)
TargetHost (URI Host)
TargetPort (URI Port)
Target (URI)
Action (URI)
Error (multiline string)