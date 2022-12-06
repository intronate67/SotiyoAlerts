# SotiyoAlerts

Discord bot to track Gurista's and Blood Raider's killmails on stargates that can pontentially represent solar systems with NPC Sotiyos.

# Deployment
Docker vs systemd but tbh probably going to need to write deployment steps for win/linux/mac w/ docker as a final solution.
Will need to compare advantages of each and present to the user it is written for linux/mac (unix).

## Docker Deployment
TBD

## systemd Deployment
An option but not the only option as I think I'll be moving to a docker implementation soon.

- Use `donet publish` to create an archive to send over to the deployment server
- Un-archive the application files in your desired location.

- Service Creation/Configuration (ubuntu linux specific)
  - Change directory to `/etc/systemd/system`
  - Run `vim SotiyoAlerts.service`
  - Inside the editor input the following code:
  ```
  [UNIT]
  Description=Whatever description you want
  [Service]
  WorkingDirectory=/<full/path/to/SotiyoAlerts DIRECTORY>
  ExecStart=/usr/bin/donet /<full/path/to/DIRECTORY/SotiyoAlerts.dll>
  Restart=always
  RestartSec=10
  SyslogIdentifier=sotiyo-alerts
  User=www-data
  Environment=ASPNETCORE_ENVIRONMENT=Production
  [Install]
  WantedBy=multi-user.target
  ```
  `esc` + `wq` and the file should be saved (may need sudo)

### systemd Commands
- Start/Stop
  - `sudo systemctl start|stop SotiyoAlerts`
- Status
  - `sudo  systemct status SotiyoAlerts`


### Deployment Location Notes (Remove on public)
- Deployed on local network on udev01
