### Bug Fixes

* prevent BackgroundFileTraceListener from queuing events infinitely when failing to write to file by adding a MaxQueueSize setting defaulted to 10000 ([88ef72c](https://github.com/zywave/SMLogging/commit/88ef72c))
* remove unecessary static locks ([5cbd964](https://github.com/zywave/SMLogging/commit/5cbd964))


### Features

* log failures to event log ([1c12355](https://github.com/zywave/SMLogging/commit/1c12355))
* log fault exception details in error handler ([8b56bb8](https://github.com/zywave/SMLogging/commit/8b56bb8))