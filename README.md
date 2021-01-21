# ERGOHAND
Bluetooth keyboard inspired by the [DataHand](https://en.wikipedia.org/wiki/DataHand).
This is a personal project to practice embedded C and low-level desktop application development, and to get some real use out of my 3D printer.

I chose the Cypress PSoC 4 platform because I already have two boards and it offers a turnkey BLE stack and built-in battery management. In the future I'd like to try a fully open-source wireless solution and cheaper microcontrollers.
## Components
### Firmware
ARM CortexM0 code. Implements a Human Interface Device over Bluetooth Low Energy and reads the key matrix inputs. Created with PSoC Creator.
### Software
A desktop GUI configuration app to provide over-the-air customization of key mapping and other device settings.
### Hardware
The only electronics are microswitches and wires, but I plan to try photointerruptors after the proof-of-concept is complete. This directory will also contain the 3D models.

