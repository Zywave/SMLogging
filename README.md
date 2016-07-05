# SMLogging (Service Model Logging)

[![Build status](https://ci.appveyor.com/api/projects/status/jq63fmhc9xspiggk?svg=true)](https://ci.appveyor.com/project/JohnCruikshank/smlogging)

This library provides request and error logging behaviors for Windows Communication Foundation (WCF / Service Model) services.  While WCF has built-in logging features, this libary is intended to provide simple, consumable log files rather than the bulky traces produced by the built-in logging.  Think of it as IIS request logging for WCF services (including net.pipe and net.tcp traffic in addition to http).
