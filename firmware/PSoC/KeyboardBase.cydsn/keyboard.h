/*********************************************
 *  File:       keyboard.h
 *  Author:     ***REMOVED***
 *  Created:    18-Jan-2021 
 *
 *  Functions and settings for interfacing with the keyboard switch hardware
 *  and managing the Human Interface Device usage reports.
 *
 ********************************************/

#ifndef KEYBOARD_H
#define	KEYBOARD_H
    
#include "common.h"
    
#define nROWS       (2u)
#define nCOLS       (4u)
#define nMAXKEYS    (24u)
#define LOOP_DELAY_MS   (1u)
#define DEBOUNCE_COUNT  (10u)

/* array that maps the physical switches to USB HID keycodes */
extern uint8_t keyMap[nMAXKEYS];  
    
inline uint8_t getKeycodeFromIndex(uint8_t row, uint8_t col){
    return keyMap[row * nCOLS + col];
}

/**
 * Polls the keyboard matrix once. Updates the keystate matrix accordingly
 *
 * Return: 1 if any switch changed state, 0 otherwise
 */
uint8_t Keyboard_ScanKeys();

/**
 * Writes the keymap variable to Flash
 *
 * Return: 0 if successful, 1 otherwise
 */
uint8_t Keyboard_WriteConfig();

/**
 * Reads keymap from Flash and updates variable
 *
 * Return: 0 if successful, 1 otherwise
 */
uint8_t Keyboard_ReadConfig();

/**
 * Notifies the HID host of a keyboard report change
 *
 * Return: result of CYBLE stack calls (CYBLE_ERROR_OK if report sent successfully)
 */
CYBLE_API_RESULT_T HID_Report_Send();

/**
 * Searches for the first instance of a keycode in the HID report and clears the status
 * TODO: move active keys to front of queue
 */
void HID_Report_RemoveKey(uint8_t keycode);

/**
 * Adds a keycode to the first available empty slot in the HID report
 */
void HID_Report_AddKey(uint8_t keycode);

#endif /* KEYBOARD_H */
/* [] END OF FILE */
