# Setup

## Hardware

Raspberry Pi 2

## OS Setup

- Download [Raspbian Lite](https://www.raspberrypi.org/downloads/raspbian/) (no desktop version) 
- Use [7Zip](http://www.7-zip.org/download.html) to extract the image
- Use [Etcher](https://etcher.io/) to download the image to the micro SD card
- Create `ssh` file on root of boot partition to enable SSH access

Insert the micro SD card and plug USB power cable. Initial setup may take a bit.

- Discover the IP address of rpi (you can use any [IP range scanner](http://angryip.org/download/) or check with your router for attached devices)
- Use [Putty](http://www.putty.org/) to SSH into your RPi using `pi` username and `raspberry` password

## Prepare RPi

### Configure Wifi on RPi

<http://weworkweplay.com/play/automatically-connect-a-raspberry-pi-to-a-wifi-network/>

- Open file `sudo nano /etc/network/interfaces`

```
auto wlan0

... other stuff

allow-hotplug wlan0
iface wlan0 inet dhcp
wpa-conf /etc/wpa_supplicant/wpa_supplicant.conf
iface default inet dhcp
```

- Fill WiFi router data `sudo nano /etc/wpa_supplicant/wpa_supplicant.conf`

Example:
```
... other stuff

network={
ssid="YOUR_NETWORK_NAME"
psk="YOUR_NETWORK_PASSWORD"
proto=RSN
key_mgmt=WPA-PSK
pairwise=CCMP
auth_alg=OPEN
}
```

- Restart the RPi `sudo shutdown -r 0`

### .NET Core Runtime

- Run `sudo apt-get -y install libunwind8 gettext`
- Run `wget https://dotnetcli.blob.core.windows.net/dotnet/Runtime/release/2.0.0/dotnet-runtime-latest-linux-arm.tar.gz`
- Run `sudo mkdir /opt/dotnet`
- Run `sudo tar -xvf dotnet-runtime-latest-linux-arm.tar.gz -C /opt/dotnet`
- Run `sudo ln -s /opt/dotnet/dotnet /usr/local/bin`
- Check if installation was successful `dotnet --info`

### Configure PlatformIO on RPi

<http://cc.bingj.com/cache.aspx?q=platformio+raspberry+pi&d=4666256022245109&mkt=en-US&setlang=en-US&w=CLrkZ6yZV39J9-2oFyPqXb4ycUE5dCbG>

- `sudo apt-get install libmpc-dev libelf1 libftdi1 python-pip`
- `sudo pip install platformio`

- You can now use following command to copy files from PC to RPi

`pscp -l pi -pw raspberry -r -C <LOCAL_FULL_DIRECTORY_PATH> 192.168.0.8:/home/pi/maii-systems-primary/`


## Prepare for .NET Core Building (PC)

- Install [.NET Core SDK](https://www.microsoft.com/net/download/core)
- Install Git (I like [Github Desktop](https://desktop.github.com/) as it comes with posh-git for Windows PowerShell)
- TODO: Get from Git? dotnet new?
- Get the [Cake](https://cakebuild.net/) build script with following command on Windows:  

`Invoke-WebRequest http://cakebuild.net/download/bootstrapper/windows -OutFile build.ps1`

- Invoke the `build.ps1` you just downloaded in Windows PowerShell
    - This will build .NET application for Linux-ARM and deploy it over SSH to the RPi
    - TODO: How to change username and password?

## Other RPi config (optional)

## Resources

### OS Setup

- <https://www.raspberrypi.org/downloads/raspbian/>
- <https://www.raspberrypi.org/documentation/installation/installing-images/README.md>
- <https://raspberrypi.stackexchange.com/a/14616>
- <https://raspberrypi.stackexchange.com/a/58628>

## Prepare for .NET Core Runtime (RPi)

- <https://jeremylindsayni.wordpress.com/2017/07/23/running-a-net-core-2-app-on-raspbian-jessie-and-deploying-to-the-pi-with-cake/>>

## Prepare for .NET Core Building (PC)

- <https://www.microsoft.com/net/download/core>
- <https://jeremylindsayni.wordpress.com/2017/07/23/running-a-net-core-2-app-on-raspbian-jessie-and-deploying-to-the-pi-with-cake/>
- <https://github.com/jeremylindsayni/RaspberryPi.Template>