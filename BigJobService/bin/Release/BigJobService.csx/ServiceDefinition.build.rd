<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="BigJobService" generation="1" functional="0" release="0" Id="c024b577-2a62-4b85-b72a-6ea92b67d331" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="BigJobServiceGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" protocol="tcp">
          <inToChannel>
            <lBChannelMoniker name="/BigJobService/BigJobServiceGroup/LB:BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="BigjobAzureAgent:?IsSimulationEnvironment?" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:?IsSimulationEnvironment?" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:?RoleHostDebugger?" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:?RoleHostDebugger?" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:?StartupTaskDebugger?" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:?StartupTaskDebugger?" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:ApplicationName" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:ApplicationName" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:DataConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:DataConnectionString" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:DiagnosticsConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:DiagnosticsConnectionString" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.ActivationToken" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.ActivationToken" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.Administrators" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.Administrators" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainAccountName" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainAccountName" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainControllerFQDN" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainControllerFQDN" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainFQDN" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainFQDN" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainOU" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainOU" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainPassword" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainPassword" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainSiteName" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainSiteName" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.EnableDomainJoin" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.EnableDomainJoin" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.Refresh" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.Refresh" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.Upgrade" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.Upgrade" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.WaitForConnectivity" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.WaitForConnectivity" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" />
          </maps>
        </aCS>
        <aCS name="BigjobAzureAgentInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapBigjobAzureAgentInstances" />
          </maps>
        </aCS>
        <aCS name="Certificate|BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" defaultValue="">
          <maps>
            <mapMoniker name="/BigJobService/BigJobServiceGroup/MapCertificate|BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput">
          <toPorts>
            <inPortMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </toPorts>
        </lBChannel>
        <sFSwitchChannel name="SW:BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp">
          <toPorts>
            <inPortMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" />
          </toPorts>
        </sFSwitchChannel>
      </channels>
      <maps>
        <map name="MapBigjobAzureAgent:?IsSimulationEnvironment?" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/?IsSimulationEnvironment?" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:?RoleHostDebugger?" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/?RoleHostDebugger?" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:?StartupTaskDebugger?" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/?StartupTaskDebugger?" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:ApplicationName" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/ApplicationName" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:DataConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/DataConnectionString" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:DiagnosticsConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/DiagnosticsConnectionString" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.ActivationToken" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.Connect.ActivationToken" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.Administrators" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.Connect.Administrators" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainAccountName" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.Connect.DomainAccountName" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainControllerFQDN" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.Connect.DomainControllerFQDN" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainFQDN" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.Connect.DomainFQDN" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainOU" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.Connect.DomainOU" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainPassword" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.Connect.DomainPassword" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.DomainSiteName" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.Connect.DomainSiteName" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.EnableDomainJoin" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.Connect.EnableDomainJoin" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.Refresh" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.Connect.Refresh" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.Upgrade" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.Connect.Upgrade" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Connect.WaitForConnectivity" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.Connect.WaitForConnectivity" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" />
          </setting>
        </map>
        <map name="MapBigjobAzureAgentInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgentInstances" />
          </setting>
        </map>
        <map name="MapCertificate|BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" kind="Identity">
          <certificate>
            <certificateMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
          </certificate>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="BigjobAzureAgent" generation="1" functional="0" release="0" software="J:\Dr Jha Project\Backup\BigJobService\bin\Release\BigJobService.csx\roles\BigjobAzureAgent" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaWorkerHost.exe " memIndex="768" hostingEnvironment="consoleroleadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" protocol="tcp" />
              <inPort name="Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" protocol="tcp" portRanges="3389" />
              <outPort name="BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" protocol="tcp">
                <outToChannel>
                  <sFSwitchChannelMoniker name="/BigJobService/BigJobServiceGroup/SW:BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" />
                </outToChannel>
              </outPort>
            </componentports>
            <settings>
              <aCS name="?IsSimulationEnvironment?" defaultValue="" />
              <aCS name="?RoleHostDebugger?" defaultValue="" />
              <aCS name="?StartupTaskDebugger?" defaultValue="" />
              <aCS name="ApplicationName" defaultValue="" />
              <aCS name="DataConnectionString" defaultValue="" />
              <aCS name="DiagnosticsConnectionString" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Connect.ActivationToken" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Connect.Administrators" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Connect.DomainAccountName" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Connect.DomainControllerFQDN" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Connect.DomainFQDN" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Connect.DomainOU" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Connect.DomainPassword" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Connect.DomainSiteName" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Connect.EnableDomainJoin" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Connect.Refresh" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Connect.Upgrade" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Connect.WaitForConnectivity" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;BigjobAzureAgent&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;BigjobAzureAgent&quot;&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
            <storedcertificates>
              <storedCertificate name="Stored0Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" certificateStore="My" certificateLocation="System">
                <certificate>
                  <certificateMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent/Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
                </certificate>
              </storedCertificate>
            </storedcertificates>
            <certificates>
              <certificate name="Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
            </certificates>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgentInstances" />
            <sCSPolicyFaultDomainMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgentFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyFaultDomain name="BigjobAzureAgentFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="BigjobAzureAgentInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="63e2dfc0-c0d4-44d3-b400-fe15a49a9865" ref="Microsoft.RedDog.Contract\ServiceContract\BigJobServiceContract@ServiceDefinition.build">
      <interfacereferences>
        <interfaceReference Id="520bd9dc-93f8-4360-96f0-12e4f33be2af" ref="Microsoft.RedDog.Contract\Interface\BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput@ServiceDefinition.build">
          <inPort>
            <inPortMoniker name="/BigJobService/BigJobServiceGroup/BigjobAzureAgent:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>