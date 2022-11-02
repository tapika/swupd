# Function can be tested in pwsh using following variable defined:
#$PSModuleAutoloadingPreference = 'None'

if( (Get-Module -Name CimCmdlets -ListAvailable) -ne $null)
{
    Import-Module CimCmdlets

    # powershell core's wrapper function can be found using commands:
    #
    # Get-Command Get-WmiObject
    # ${function:Get-WmiObject}.File

    # This is a wrapper function from Get-WmiObject to Get-CimInstance
    # not all parameters were verified but commonly used ones
    function Get-WmiObject(
        [Alias('ClassName')]
        ${Class},

        [string]$Filter = $null
    )
    {
        if($PSBoundParameters.ContainsKey('Filter'))
        {
            return Get-CimInstance -ClassName $Class -Filter $Filter
        }
        
        return Get-CimInstance -ClassName $Class
    }
}

