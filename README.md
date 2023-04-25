# Azure Application Insights Module

## Overview

Azure Application Insights module writes Virto Commerce events and telemetry to Microsoft Application Insights using Serilog. 

## Key Features
* Built with [Serilog](https://serilog.net/)
* Sends events data to Microsoft Azure Application Insights.
* Sends telemetry data to Microsoft Azure Application Insights.
* Suppors configuration by config and custom module.

## Configuring

The simplest way to configure Serilog to send data to an Application Insights dashboard via instrumentation key is to use current active telemetry configuration which is already initialized in most application types like ASP.NET Core.

This module supports configuration by config and code. You can read more about configuration [here](https://github.com/serilog-contrib/serilog-sinks-applicationinsights)


## References
* Deployment: https://docs.virtocommerce.org/developer-guide/deploy-module-from-source-code/
* Installation: https://docs.virtocommerce.org/user-guide/modules/
* Home: https://virtocommerce.com
* Community: https://www.virtocommerce.org
* [Download Latest Release](https://github.com/VirtoCommerce/vc-module-app-insights/releases/latest)

## License

Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

<http://virtocommerce.com/opensourcelicense>

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
