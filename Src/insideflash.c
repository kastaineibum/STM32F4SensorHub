#include "insideflash.h"
#include "string.h"
#include <stdio.h>

#define FLASH_DEBUG 1
#if FLASH_DEBUG
#define FLASH_INFO( fmt, args... ) 	printf( fmt, ##args )//KEY_VALUE_INFO(fmt, ##args)
#else
#define FLASH_INFO( fmt, args... )
#endif

bool flash_erase( int32_t flashaddr, uint32_t page ){
    
    if( ( ( flashaddr < FLASH_BASE || flashaddr > FLASH_END_ADDR ) && flash_legal_sector_address( flashaddr ) ) || page == 0){
        FLASH_INFO("flash_erase: erase para error\r\n");
        return false;
    }

    FLASH_EraseInitTypeDef f;
    
    #if defined CORTEX_M3
        f.TypeErase = FLASH_TYPEERASE_PAGES;
        
        f.PageAddress = flashaddr;
        
        f.NbPages = page;
	#else
        f.TypeErase = FLASH_TYPEERASE_SECTORS;
        
        f.Sector = flash_sector_index( flashaddr ) ;
        
        f.NbSectors = page;
        
        f.VoltageRange = FLASH_VOLTAGE_RANGE_3;
        
        f.Banks = FLASH_BANK_1;
    #endif
    
    uint8_t cycleCount = 0;

    uint32_t PageError = 0;
    
reerase:
	
    __disable_irq();
    HAL_FLASH_Unlock();
    
    #if defined CORTEX_M3
        #if !defined _STM32L_
            __HAL_FLASH_CLEAR_FLAG( FLASH_FLAG_BSY | FLASH_FLAG_EOP | FLASH_FLAG_PGERR | FLASH_FLAG_WRPERR );
        #else
            __HAL_FLASH_CLEAR_FLAG( FLASH_FLAG_BSY | FLASH_FLAG_EOP | FLASH_FLAG_WRPERR | FLASH_FLAG_PGAERR | FLASH_FLAG_SIZERR | FLASH_FLAG_OPTVERR );
        #endif
    #else
        __HAL_FLASH_CLEAR_FLAG( FLASH_FLAG_BSY | FLASH_FLAG_EOP | FLASH_FLAG_PGSERR | FLASH_FLAG_PGPERR | FLASH_FLAG_PGAERR | FLASH_FLAG_WRPERR );
    #endif
    
	HAL_FLASHEx_Erase(&f, &PageError);
    
    HAL_FLASH_Lock();
    __enable_irq();
	
    if( PageError != 0xFFFFFFFF ){
        cycleCount ++;
        if( cycleCount >= 5 ){
            FLASH_INFO("erase failed\r\n");
            return false;
        }
        HAL_Delay(100);
        goto reerase;
    }
    
//    uint8_t *eraseaddr = (uint8_t *)flashaddr;
//    for( uint32_t i = 0; i < page * KEY_VALUE_SIZE ; i ++ ){
//        if( eraseaddr[ i ] != (uint8_t) ERASURE_STATE ){
//            return false;
//        }
//    }
    
    return true;
}

bool flash_write( const uint8_t *ramaddr, uint32_t flashaddr, int32_t size ){
    
    if( ( flashaddr < FLASH_BASE || flashaddr > FLASH_END_ADDR ) || size == 0){
        FLASH_INFO("flash_write: para error\r\n");
        return false;
    }
    
    if( ( ( flashaddr - FLASH_BASE ) % 4) != 0 ){
		FLASH_INFO("flash_write: Writing a int address is wrong\r\n");
        return false;
    }
	
//	if( size > KEY_VALUE_SIZE ){
//		FLASH_INFO("The amount of data is too large\r\n");
//		return false;
//	}
	
    uint32_t currentflashAddr   = flashaddr;
    const uint8_t  *currentram  = ramaddr;
    uint8_t  Remainder          = size % 4;
    uint16_t Multiple           = size / 4;
	
    __disable_irq();
    HAL_FLASH_Unlock();
    
    #if defined CORTEX_M3
        #if !defined _STM32L_
            __HAL_FLASH_CLEAR_FLAG( FLASH_FLAG_BSY | FLASH_FLAG_EOP | FLASH_FLAG_PGERR | FLASH_FLAG_WRPERR );
        #else
            __HAL_FLASH_CLEAR_FLAG( FLASH_FLAG_BSY | FLASH_FLAG_EOP | FLASH_FLAG_WRPERR | FLASH_FLAG_PGAERR | FLASH_FLAG_SIZERR | FLASH_FLAG_OPTVERR );
        #endif
    #else
        __HAL_FLASH_CLEAR_FLAG( FLASH_FLAG_BSY | FLASH_FLAG_EOP | FLASH_FLAG_PGSERR | FLASH_FLAG_PGPERR | FLASH_FLAG_PGAERR | FLASH_FLAG_WRPERR );
    #endif
    
    for( int32_t i = 0; i < Multiple ; i ++  ){
        HAL_FLASH_Program( FLASH_TYPEPROGRAM_WORD, currentflashAddr, *((uint32_t *)currentram) );
        currentflashAddr += 4;
        currentram += 4;
    }
    
    if( Remainder ){
        uint8_t data[4] ;
		memset( data, 0x00, 4);//change
        for( uint8_t i = 0; i < Remainder; i++ ){
            data[i] = currentram[i];
        }
        HAL_FLASH_Program( FLASH_TYPEPROGRAM_WORD, currentflashAddr,  *((uint32_t *)data) );
    }
    
    HAL_FLASH_Lock();
	__enable_irq();
    
    #if defined _STM32L_
        return true;
    #else
        if( memcmp( ramaddr, (uint8_t *)flashaddr, size) == 0 ){
            return true;
        }else{
            FLASH_INFO("inside flash write failure\r\n");
            return false;
        }
    #endif
    
}



