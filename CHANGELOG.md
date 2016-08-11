<a name="1.3.3"></a>
## [1.3.3](https://github.com/zywave/SMLogging/compare/1.3.2...v1.3.3) (2016-08-11)


### Bug Fixes

* remove invalid file name characters ([3a50684](https://github.com/zywave/SMLogging/commit/3a50684))



<a name="1.3.2"></a>
## [1.3.2](https://github.com/zywave/SMLogging/compare/1.3.1...v1.3.2) (2016-08-08)


### Bug Fixes

* remove duplicate activity id ([fb149ca](https://github.com/zywave/SMLogging/commit/fb149ca))



<a name="1.3.1"></a>
## [1.3.1](https://github.com/zywave/SMLogging/compare/1.3.0...v1.3.1) (2016-08-03)


### Bug Fixes

* removed old attributes from install action ([7a8724d](https://github.com/zywave/SMLogging/commit/7a8724d))



<a name="1.3.0"></a>
# [1.3.0](https://github.com/zywave/SMLogging/compare/1.2.5...v1.3.0) (2016-08-03)


### Features

* activity tracing ([d2b6e85](https://github.com/zywave/SMLogging/commit/d2b6e85))
* end to end tracing enabled via installer ([f999403](https://github.com/zywave/SMLogging/commit/f999403))


### BREAKING CHANGES

* added activityid and correlationid to format



<a name="1.2.5"></a>
## [1.2.5](https://github.com/zywave/SMLogging/compare/1.2.4...v1.2.5) (2016-07-28)


### Bug Fixes

* prevent setting unsupported messageid header ([11aefe9](https://github.com/zywave/SMLogging/commit/11aefe9))



<a name="1.2.4"></a>
## [1.2.4](https://github.com/zywave/SMLogging/compare/1.2.3...v1.2.4) (2016-07-21)


### Bug Fixes

* add option to ignore dispatch reply ([fdb1e4b](https://github.com/zywave/SMLogging/commit/fdb1e4b))
* fault requests marked as faults ([e010de0](https://github.com/zywave/SMLogging/commit/e010de0))



<a name="1.2.3"></a>
## [1.2.3](https://github.com/zywave/SMLogging/compare/1.2.2...v1.2.3) (2016-07-20)


### Bug Fixes

* setup properly versioned on release ([1162160](https://github.com/zywave/SMLogging/commit/1162160))



<a name="1.2.2"></a>
## [1.2.2](https://github.com/zywave/SMLogging/compare/1.2.0...v1.2.2) (2016-07-20)


### Bug Fixes

* log all errors (not just faults) ([4dde907](https://github.com/zywave/SMLogging/commit/4dde907))
* wsdl requests are not treated as oneway ([219f91d](https://github.com/zywave/SMLogging/commit/219f91d))



<a name="1.2.0"></a>
# [1.2.0](https://github.com/zywave/SMLogging/compare/1.1.1...1.2.0) (2016-07-15)


### Bug Fixes

* error logging messageId ([aef62a8](https://github.com/zywave/SMLogging/commit/aef62a8))
* sign on release only ([c6017e4](https://github.com/zywave/SMLogging/commit/c6017e4))


### Features

* commonBehaviors instead of nameless behaviors ([c28eec5](https://github.com/zywave/SMLogging/commit/c28eec5))
* endpoint behavior for client logging ([ea04ccf](https://github.com/zywave/SMLogging/commit/ea04ccf))
* error handler endpoint behavior ([0d08b76](https://github.com/zywave/SMLogging/commit/0d08b76))
* flag for adding request header ([f28df9f](https://github.com/zywave/SMLogging/commit/f28df9f))
* include activity id ([418d240](https://github.com/zywave/SMLogging/commit/418d240))
* install endpoint behavior ([37202b6](https://github.com/zywave/SMLogging/commit/37202b6))



<a name="1.1.1"></a>
## [1.1.1](https://github.com/zywave/SMLogging/compare/1.1.0...1.1.1) (2016-07-11)


### Bug Fixes

* net40 dependency group ([37185fe](https://github.com/zywave/SMLogging/commit/37185fe))



<a name="1.1.0"></a>
# [1.1.0](https://github.com/zywave/SMLogging/compare/1.0.3...1.1.0) (2016-07-11)


### Features

* net35 support ([c589760](https://github.com/zywave/SMLogging/commit/c589760))



<a name="1.0.3"></a>
## [1.0.3](https://github.com/zywave/SMLogging/compare/1.0.2...1.0.3) (2016-07-08)


### Bug Fixes

* uninstall custom actions on upgrade ([c8d3cc6](https://github.com/zywave/SMLogging/commit/c8d3cc6))



<a name="1.0.2"></a>
## [1.0.2](https://github.com/zywave/SMLogging/compare/1.0.1...1.0.2) (2016-07-08)


### Bug Fixes

* full assembly version ([d8ccf7a](https://github.com/zywave/SMLogging/commit/d8ccf7a))
* product version on assembly name ([7df0e27](https://github.com/zywave/SMLogging/commit/7df0e27))



<a name="1.0.1"></a>
## [1.0.1](https://github.com/zywave/SMLogging/compare/1.0.0...1.0.1) (2016-07-08)


### Bug Fixes

* null checking reply (one way call) ([a5c9228](https://github.com/zywave/SMLogging/commit/a5c9228))



<a name="1.0.0"></a>
# [1.0.0](https://github.com/zywave/SMLogging/compare/bdd6de8...1.0.0) (2016-07-06)


### Bug Fixes

* builds ([6caacbb](https://github.com/zywave/SMLogging/commit/6caacbb))
* call correct register methods ([a041538](https://github.com/zywave/SMLogging/commit/a041538))
* corrected strong name token ([6d95719](https://github.com/zywave/SMLogging/commit/6d95719))
* datetimes ([6699f46](https://github.com/zywave/SMLogging/commit/6699f46))
* flush both queue and stream on close ([e40800a](https://github.com/zywave/SMLogging/commit/e40800a))
* get details from providefault method instead of handleerror ([d8d79ae](https://github.com/zywave/SMLogging/commit/d8d79ae))
* installation fixes ([e34633e](https://github.com/zywave/SMLogging/commit/e34633e))
* provide default values for null fields ([e111533](https://github.com/zywave/SMLogging/commit/e111533))
* public signing key ([f99f6fd](https://github.com/zywave/SMLogging/commit/f99f6fd))


### Features

* behavior attribute ([5416799](https://github.com/zywave/SMLogging/commit/5416799))
* error logging ([2fe2b2b](https://github.com/zywave/SMLogging/commit/2fe2b2b))
* installer ([bdd6de8](https://github.com/zywave/SMLogging/commit/bdd6de8))
* strong name signing CI ([56d1641](https://github.com/zywave/SMLogging/commit/56d1641))



