/*********************************************
 *  File:       main.c
 *  Created:    18-Jan-2021 
 *
 *  Bluetooth Low Energy keyboard project. 
 *  Full documentation at:
 *  https://github.com/aspck/ErgoHand 
 *
 ********************************************/
#include "keyboard.h"

/** Cypress headers */
#include "project.h"
#include "debug.h"
#include "hids.h"

/** Constants */
#define LOOP_DELAY_MS   (1u)

/** Global Variables */
uint8_t mainTimer = 0;
uint8_t Keyboard_configChanged = 0;    
uint8_t Keyboard_configReadOnce = 1;

/* dynamic memory location for simulated EEPROM */
const uint8_t EEPROM_mem[EEPROM_PHYSICAL_SIZE]
__ALIGNED(CY_FLASH_SIZEOF_ROW) = {0u};

/** Interrupt Service Routine: Timer */
CY_ISR(ISR_Timer)
{
    mainTimer++;
    
    //clear interrupt flag
    Timer_ReadStatusRegister();    
}

/** BLE event handler */
void BLECallBack(uint32 event, void* eventParam)
{
    CYBLE_API_RESULT_T apiResult;
    CYBLE_GAP_BD_ADDR_T localAddr;
    CYBLE_GAP_AUTH_INFO_T *authInfo;
    CYBLE_GAP_SMP_KEY_DIST_T Keys;
    uint8 i;
    
    switch (event)
	{
        /**********************************************************
        *                       General Events
        ***********************************************************/
		case CYBLE_EVT_STACK_ON: /* This event is received when the component is Started */
            /* Enter into discoverable mode so that remote can search it. */
        CyBle_GapGenerateKeys(0x77,&Keys);
            apiResult = CyBle_GappStartAdvertisement(CYBLE_ADVERTISING_FAST);
            if(apiResult != CYBLE_ERROR_OK)
            {
                DBG_PRINTF("StartAdvertisement API Error: %d \r\n", apiResult);
            }
            DBG_PRINTF("Bluetooth On, StartAdvertisement with addr: ");
            Advertising_LED_Write(LED_ON);
            localAddr.type = 0u;
            CyBle_GetDeviceAddress(&localAddr);
            for(i = CYBLE_GAP_BD_ADDR_SIZE; i > 0u; i--)
            {
                DBG_PRINTF("%2.2x", localAddr.bdAddr[i-1]);
            }
            DBG_PRINTF("\r\n");
            break;
		case CYBLE_EVT_TIMEOUT: 
            break;
		case CYBLE_EVT_HARDWARE_ERROR:    /* This event indicates that some internal HW error has occurred. */
            DBG_PRINTF("CYBLE_EVT_HARDWARE_ERROR \r\n");
			break;
            
    	/* This event will be triggered by host stack if BLE stack is busy or not busy.
    	 *  Parameter corresponding to this event will be the state of BLE stack.
    	 *  BLE stack busy = CYBLE_STACK_STATE_BUSY,
    	 *  BLE stack not busy = CYBLE_STACK_STATE_FREE 
         */
    	case CYBLE_EVT_STACK_BUSY_STATUS:
            DBG_PRINTF("CYBLE_EVT_STACK_BUSY_STATUS: %x\r\n", *(uint8 *)eventParam);
            break;
        case CYBLE_EVT_HCI_STATUS:
            DBG_PRINTF("CYBLE_EVT_HCI_STATUS: %x \r\n", *(uint8 *)eventParam);
			break;
            
        /**********************************************************
        *                       GAP Events
        ***********************************************************/
        case CYBLE_EVT_GAP_AUTH_REQ:
            DBG_PRINTF("CYBLE_EVT_AUTH_REQ: security=%x, bonding=%x, ekeySize=%x, err=%x \r\n", 
                (*(CYBLE_GAP_AUTH_INFO_T *)eventParam).security, 
                (*(CYBLE_GAP_AUTH_INFO_T *)eventParam).bonding, 
                (*(CYBLE_GAP_AUTH_INFO_T *)eventParam).ekeySize, 
                (*(CYBLE_GAP_AUTH_INFO_T *)eventParam).authErr);
            break;
        case CYBLE_EVT_GAP_PASSKEY_ENTRY_REQUEST:
            DBG_PRINTF("CYBLE_EVT_PASSKEY_ENTRY_REQUEST press 'p' to enter passkey \r\n");
            break;
        case CYBLE_EVT_GAP_PASSKEY_DISPLAY_REQUEST:
            DBG_PRINTF("CYBLE_EVT_PASSKEY_DISPLAY_REQUEST %6.6ld \r\n", *(uint32 *)eventParam);
            break;
        case CYBLE_EVT_GAP_KEYINFO_EXCHNGE_CMPLT:
            DBG_PRINTF("CYBLE_EVT_GAP_KEYINFO_EXCHNGE_CMPLT \r\n");
            break;
        case CYBLE_EVT_GAP_AUTH_COMPLETE:
            authInfo = (CYBLE_GAP_AUTH_INFO_T *)eventParam;
            (void)authInfo;
            DBG_PRINTF("AUTH_COMPLETE: security:%x, bonding:%x, ekeySize:%x, authErr %x \r\n", 
                                    authInfo->security, authInfo->bonding, authInfo->ekeySize, authInfo->authErr);
            break;
        case CYBLE_EVT_GAP_AUTH_FAILED:
            DBG_PRINTF("CYBLE_EVT_AUTH_FAILED: %x \r\n", *(uint8 *)eventParam);
            break;
        case CYBLE_EVT_GAPP_ADVERTISEMENT_START_STOP:
            DBG_PRINTF("CYBLE_EVT_ADVERTISING, state: %x \r\n", CyBle_GetState());
            if(CYBLE_STATE_DISCONNECTED == CyBle_GetState())
            {   
                /* Fast and slow advertising period complete, go to low power  
                 * mode (Hibernate mode) and wait for an external
                 * user event to wake up the device again */
                DBG_PRINTF("Hibernate \r\n");
                
                Advertising_LED_Write(LED_OFF);/*
                Disconnect_LED_Write(LED_ON);
                CapsLock_LED_Write(LED_OFF);
                SW2_ClearInterrupt();
                Wakeup_Interrupt_ClearPending();
                Wakeup_Interrupt_Start();
                */
            #if (DEBUG_UART_ENABLED == ENABLED)
                /* Wait until debug info is sent */
                while((UART_DEB_SpiUartGetTxBufferSize() + UART_DEB_GET_TX_FIFO_SR_VALID) != 0);
            #endif /* (DEBUG_UART_ENABLED == ENABLED) */
                CySysPmHibernate();
            }
            break;
        case CYBLE_EVT_GAP_DEVICE_CONNECTED:
            DBG_PRINTF("CYBLE_EVT_GAP_DEVICE_CONNECTED \r\n");
            Advertising_LED_Write(LED_OFF);
            break;
        case CYBLE_EVT_GAP_DEVICE_DISCONNECTED:
            DBG_PRINTF("CYBLE_EVT_GAP_DEVICE_DISCONNECTED\r\n");
            apiResult = CyBle_GappStartAdvertisement(CYBLE_ADVERTISING_FAST);
            if(apiResult != CYBLE_ERROR_OK)
            {
                DBG_PRINTF("StartAdvertisement API Error: %d \r\n", apiResult);
            }
            break;
        case CYBLE_EVT_GATTS_XCNHG_MTU_REQ:
            { 
                uint16 mtu;
                CyBle_GattGetMtuSize(&mtu);
                DBG_PRINTF("CYBLE_EVT_GATTS_XCNHG_MTU_REQ, final mtu= %d \r\n", mtu);
            }
            break;/*
        case CYBLE_EVT_GATTS_WRITE_REQ:
            DBG_PRINTF("CYBLE_EVT_GATT_WRITE_REQ: %x = ",((CYBLE_GATTS_WRITE_REQ_PARAM_T *)eventParam)->handleValPair.attrHandle);
            ShowValue(&((CYBLE_GATTS_WRITE_REQ_PARAM_T *)eventParam)->handleValPair.value);
            (void)CyBle_GattsWriteRsp(((CYBLE_GATTS_WRITE_REQ_PARAM_T *)eventParam)->connHandle);
            break;/*/
        case CYBLE_EVT_GAP_ENCRYPT_CHANGE:
            DBG_PRINTF("CYBLE_EVT_GAP_ENCRYPT_CHANGE: %x \r\n", *(uint8 *)eventParam);
            break;
        case CYBLE_EVT_GAPC_CONNECTION_UPDATE_COMPLETE:
            DBG_PRINTF("CYBLE_EVT_CONNECTION_UPDATE_COMPLETE: %x \r\n", *(uint8 *)eventParam);
            break;
            
        /**********************************************************
        *                       GATT Events
        ***********************************************************/
        case CYBLE_EVT_GATT_CONNECT_IND:
            DBG_PRINTF("CYBLE_EVT_GATT_CONNECT_IND: %x, %x \r\n", cyBle_connHandle.attId, cyBle_connHandle.bdHandle);
            /* Register service specific callback functions */
            
            HidsInit();
            break;
        case CYBLE_EVT_GATT_DISCONNECT_IND:
            DBG_PRINTF("CYBLE_EVT_GATT_DISCONNECT_IND \r\n");
            break;
        case CYBLE_EVT_GATTS_READ_CHAR_VAL_ACCESS_REQ:
            /* Triggered on server side when client sends read request and when
            * characteristic has CYBLE_GATT_DB_ATTR_CHAR_VAL_RD_EVENT property set. */
            DBG_PRINTF("CYBLE_EVT_GATTS_READ_CHAR_VAL_ACCESS_REQ: handle: %x \r\n", ((CYBLE_GATTS_CHAR_VAL_READ_REQ_T *)eventParam)->attrHandle);              
            break;
        case CYBLE_EVT_GATTS_WRITE_CMD_REQ:
            /** 'Write Command' Request from client device. Event parameter is a 
            pointer to a structure of type CYBLE_GATTS_WRITE_CMD_REQ_PARAM_T. */
            DBG_PRINTF("CYBLE_EVT_GATTS_WRITE_CMD_REQ: handle: %x\r\n", ((CYBLE_GATTS_WRITE_CMD_REQ_PARAM_T *)eventParam)->handleValPair.attrHandle);
            break;
        case CYBLE_EVT_GATTS_PREP_WRITE_REQ:            
            /** 'Prepare Write' Request from client device. Event parameter is a
            pointer to a structure of type CYBLE_GATTS_PREP_WRITE_REQ_PARAM_T. */
            DBG_PRINTF("CYBLE_EVT_GATTS_PREP_WRITE_REQ \r\n");
            break;
         
    int16 i;
    
            
        case CYBLE_EVT_GATTS_WRITE_REQ:
            {
                CYBLE_GATT_HANDLE_VALUE_PAIR_T handle = ((CYBLE_GATTS_WRITE_REQ_PARAM_T *)eventParam)->handleValPair;
                DBG_PRINTF("CYBLE_EVT_GATTS_WRITE_REQ; handle: %x\r\n", handle.attrHandle);
                
                if (handle.attrHandle == KEYMAP_ATTR1)
                {
                    for (int w = 0; w < handle.value.len; w++)
                    {
                        keyMap[w] = handle.value.val[w];
                        DBG_PRINTF("%02x:", handle.value.val[w]);
                    }
                    Keyboard_configChanged = 1;
                }
                else if (handle.attrHandle == KEYMAP_ATTR2)
                {
                    for (int w = 0; w < handle.value.len; w++)
                    {
                        keyMap[w+(nMAXKEYS/2)] = handle.value.val[w];
                        DBG_PRINTF("%02x:", handle.value.val[w]);
                    }
                    Keyboard_configChanged = 1;
                }
                                    
                (void)CyBle_GattsWriteRsp(((CYBLE_GATTS_WRITE_REQ_PARAM_T *)eventParam)->connHandle);
            }  
        break;
        /* LONG attribute write from client */    
        case CYBLE_EVT_GATTS_EXEC_WRITE_REQ:
            {
                CYBLE_GATTS_EXEC_WRITE_REQ_T* e = (CYBLE_GATTS_EXEC_WRITE_REQ_T *)eventParam;
                CYBLE_GATT_HANDLE_VALUE_OFFSET_PARAM_T* offset = e->baseAddr;
                uint8_t numberOfPackets = e->prepWriteReqCount;
                
                DBG_PRINTF("CYBLE_EVT_GATTS_EXEC_WRITE_REQ; handle: %x\r\n", offset->handleValuePair.attrHandle);
                
                /* check which keymap attribute it is */
                if (offset->handleValuePair.attrHandle == KEYMAP_ATTR1)
                {                        
                    uint8_t arrayLength = 0;
                    for (int w = 0; w < numberOfPackets; w++)
                    {
                        arrayLength += offset[w].handleValuePair.value.len;
                    }            
                    DBG_PRINTF("length: %i\r\n", arrayLength);     

                    for (int w = 0; w < arrayLength; w++)
                    {
                        keyMap[w] = offset->handleValuePair.value.val[w];
                        DBG_PRINTF("%02x:", offset->handleValuePair.value.val[w]);
                    }
                    DBG_PRINTF("\r\n");
                    
                    /* notify main loop that config needs to be saved in flash */                       
                    Keyboard_configChanged = 1;
                }
                else if (offset->handleValuePair.attrHandle == KEYMAP_ATTR2)
                {                        
                    uint8_t arrayLength = 0;
                    for (int w = 0; w < numberOfPackets; w++)
                    {
                        arrayLength += offset[w].handleValuePair.value.len;
                    }            
                    DBG_PRINTF("length: %i\r\n", arrayLength);     

                    for (int w = 0; w < arrayLength; w++)
                    {
                        keyMap[w+(nMAXKEYS/2)] = offset->handleValuePair.value.val[w];
                        DBG_PRINTF("%02x:", offset->handleValuePair.value.val[w]);
                    }
                    DBG_PRINTF("\r\n");
                    
                    /* notify main loop that config needs to be saved in flash */                       
                    Keyboard_configChanged = 1;
                }        
            }                 
        break;
        /**********************************************************
        *                       Other Events
        ***********************************************************/
		case CYBLE_EVT_PENDING_FLASH_WRITE:
            /* Inform application that flash write is pending. Stack internal data 
            * structures are modified and require to be stored in Flash using 
            * CyBle_StoreBondingData() */
            DBG_PRINTF("CYBLE_EVT_PENDING_FLASH_WRITE\r\n");
            break;

        default:
            DBG_PRINTF("OTHER event: %lx \r\n", event);
			break;
	}

}

int main(void)
{
    CyGlobalIntEnable; /* Enable global interrupts. */
    
    UART_DEB_Start(); /* start Serial Debugger, can't use PRINTF without this */
    DBG_PRINTF("BLE Keyboard entered main\r\n");      

    CyBle_Start(BLECallBack); /* start BLE stack */
    Timer_Start();
    ISR_tmr_StartEx(ISR_Timer); /* register timer interrupt service routine */    
    EEPROM_Init((uint32_t)EEPROM_mem); /* initialize emulated EEPROM */

    for(;;)
    {      
        CyBle_ProcessEvents(); /* required BLE function */
         
        /* check for Bluetooth connection*/
        if ((CyBle_GetState() == CYBLE_STATE_CONNECTED) && (suspend != CYBLE_HIDS_CP_SUSPEND))
        {
            /* main logic loop controlled by Timer */
            if (mainTimer > LOOP_DELAY_MS)
            {
                mainTimer = 0;
                
                /* check if HID service is active */
                if (HIDS_status == ENABLED)
                {
                    /* poll keys and check for state change */
                    if (Keyboard_ScanKeys())
                    {
                        DBG_PRINTF("Key event detected\r\n");
                        
                        /* send HID report with updated key state(s) */
                        CYBLE_API_RESULT_T apiResult = HID_Report_Send();
                        if (apiResult != CYBLE_ERROR_OK)
                        {
                            DBG_PRINTF("HID notification API Error: %x \r\n", apiResult);   
                        }
                        else
                        {
                            DBG_PRINTF("HID notification success\r\n");
                        }
                    }  
                }
            }                 
            
            /** Flash operations. Only done after Bluetooth connection has been made
             *  for the sake of minimizing write cycles. */
            
            /* Read keyboard configuration from Flash, only done once per power cycle */
            if (Keyboard_configReadOnce)
            {
                if(Keyboard_ReadConfig() == 0)
                {
                    Keyboard_configReadOnce = 0;
                    /* update GATT attribute to reflect configuration from EEPROM */
                    Keyboard_UpdateConfigAtt();
                    DBG_PRINTF("Config read from EEPROM\r\n");
                } else {
                    DBG_PRINTF("Error: First-time EEPROM read failed\r\n");
                }
            }
            
            /* check if config needs to be written */
            if (Keyboard_configChanged)
            {
                /* attempt to write to flash */
                if (Keyboard_WriteConfig() == 0)
                {
                    Keyboard_configChanged = 0;
                    Keyboard_UpdateConfigAtt();
                    DBG_PRINTF("New device config saved to EEPROM\r\n");
                } else {
                    DBG_PRINTF("Error: New device config not saved, reset device\r\n");
                    /* should probably do something here... if EEPROM doesn't work something is very wrong */
                    while(1);
                }       
            }
            
            /* Store bonding data to flash, this will only run once per pairing */
            #if(CYBLE_BONDING_REQUIREMENT == CYBLE_BONDING_YES)
            #if (DEBUG_UART_ENABLED == ENABLED)
                if((cyBle_pendingFlashWrite != 0u) &&
                   ((UART_DEB_SpiUartGetTxBufferSize() + UART_DEB_GET_TX_FIFO_SR_VALID) == 0u))
                
            #else
                if(cyBle_pendingFlashWrite != 0u)
            #endif /* (DEBUG_UART_ENABLED == ENABLED) */
                {
                    CYBLE_API_RESULT_T apiResult;
                    
                    apiResult = CyBle_StoreBondingData(0u);
                    (void)apiResult;
                    DBG_PRINTF("Store bonding data, status: %x \r\n", apiResult);
                }
            #endif /* CYBLE_BONDING_REQUIREMENT == CYBLE_BONDING_YES */                         
        }        
    }
}

/* [] END OF FILE */
