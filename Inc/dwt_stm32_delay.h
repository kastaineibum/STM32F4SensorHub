#ifndef DWT_STM32_DELAY_H
#define DWT_STM32_DELAY_H
#ifdef __cplusplus
extern 'C' {
#endif
     
#include "stm32f4xx_hal.h"
/**
* @brief Initializes DWT_Cycle_Count for DWT_Delay_us function
* @return Error DWT counter
* 1: DWT counter Error
* 0: DWT counter works
*/
uint32_t DWT_Delay_Init(void);
void delay_ms(uint32_t ms);
void delay_us(uint32_t us);
void mget_ms(unsigned long *time);
uint32_t get_cyccnt(void);
	
/**
* @brief This function provides a delay (in useconds)
* @param useconds: delay in microseconds/us
*/
__STATIC_INLINE void DWT_Delay_us(volatile uint32_t useconds)
{
     uint32_t clk_cycle_start = DWT->CYCCNT;
     /* Go to number of cycles for system */
     useconds *= (HAL_RCC_GetHCLKFreq() / 1000000);
     /* Delay till end */
     while ((DWT->CYCCNT - clk_cycle_start) < useconds);
}
     
#ifdef __cplusplus
}
#endif
#endif
