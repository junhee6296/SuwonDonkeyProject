# SSH password hotfix

- Fixed older generated VirtualBox SSH commands that still contained `BatchMode=yes` and `NumberOfPasswordPrompts=0`.
- When an SSH password is stored from the command generator, the runner now strips those options just before execution and forces OpenSSH password authentication.
- Removed the temporary plaintext password file approach. The password is passed only through the child process environment variable `TEAMAPP_SSH_PASSWORD` and returned by a temporary `SSH_ASKPASS` script.
- Command preview/log still masks the password.

Important: if the SSH password box is empty or auto password input was not enabled when generating the command, the command will still fail fast instead of waiting for an invisible password prompt.
