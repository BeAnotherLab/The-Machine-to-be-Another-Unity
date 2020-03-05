### CONTROLINO MAXI Bootloader burn guide v 01.00, 2017-05-05                      

1. Connect an ISP programmer (e.g. Atmel-ICE) to the CONTROLLINO pin header connector X1

   ` TCK - SCK`
   
   ` GND - GND`
   
   ` TDO - MISO`
   
   ` VTref - VCC (5V!)`
   
   ` nSRST - Reset`
   
   ` TDI - MOSI`
   

2. Connect the ISP programmer to your PC

3. Provide power for your CONTROLLINO (via USB, or by external power supply)

4. Start Atmel Studio

5. Select Tools-Device programming

   Device - ATmega2560, Interface - ISP, verify signature and voltage!

6. Select Tools-Device programming - Memories. In flash part find path to Bootloader_CONTROLLINO_MAXI.hex dwonloaded from GitHub.

7. Click program.

8. OPTIONAL - Configure and flash fuses to following values:

   ` EXTENDED = 0xFD`

   ` HIGH = 0xD8`

   ` LOW = 0xFF` 

### Bootloader_CONTROLLINO_MAXI Release Notes
* Current version: 1.0 (8.1.2016)
* Modified original ATmega2560 bootloader - removed using LEDONBOARD while uploading new sketches.