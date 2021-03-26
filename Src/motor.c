/**
  * Board Designer				3mn.net demotree.vip  
	* All code between comment pairs USER CODE BEGIN and USER CODE END 
	* are written by Board Designer.
	* 
	* Copyright (c) 2020 Beijing Lizishu Tech Co.,Ltd.
  * All rights reserved.
  */
	
	
#include "motor.h"
#include "dwt_stm32_delay.h"

void motor_initpin(uint16_t motorid,uint16_t bgrb,uint16_t pinnum)
{
	// bgrb 0 A+ 1 A- 2 B+ 3 B-     stopswitch
	if(motorid<16&&bgrb<6)
	{
		motorpin[motorid][bgrb]=pinnum;
		setiolow(pinnum);
		setioout(pinnum);
		setiolow(pinnum);
	}
	else if(motorid<16&&bgrb==LSTOPPIN)
	{
		motorpin[motorid][bgrb]=pinnum;
		setiolow(pinnum);
		setioin(pinnum);
	}
	else if(motorid<16&&bgrb==RSTOPPIN)
	{
		motorpin[motorid][bgrb]=pinnum;
		setiolow(pinnum);
		setioin(pinnum);
	}
	else
	{
		
	}
}

void motor42_fround(uint16_t motorid,uint32_t round)
{
	motor42_forward(motorid,round*50);
}

void motor42_rround(uint16_t motorid,uint32_t round)
{
	motor42_reverse(motorid,round*50);
}

void motor42_forward(uint16_t motorid,uint32_t step)
{
	if(motorpin[motorid][0]!=0&&motorpin[motorid][1]!=0&&motorpin[motorid][2]!=0&&motorpin[motorid][3]!=0)
	{
		setiolow(motorpin[motorid][0]);setiolow(motorpin[motorid][1]);setiolow(motorpin[motorid][2]);setiolow(motorpin[motorid][3]);
		for(int i=0;i<step;i++)
		{
			if(getiostate(motorpin[motorid][LSTOPPIN])==1)
			{
				for(int j=0;j<BACKSTEP;j++)
				{
					// A 1000,B-A 1001,B- 0001,BB- 0101,B 0100,A-B 0110,A- 0010,AA- 1010. 
					setiohigh(motorpin[motorid][0]);setiolow(motorpin[motorid][2]);
					delay_ms(MOTORTICKGAP);
					setiohigh(motorpin[motorid][3]);
					delay_ms(MOTORTICKGAP);
					setiolow(motorpin[motorid][0]);
					delay_ms(MOTORTICKGAP);
					setiohigh(motorpin[motorid][1]);
					delay_ms(MOTORTICKGAP);
					setiolow(motorpin[motorid][3]);
					delay_ms(MOTORTICKGAP);
					setiohigh(motorpin[motorid][2]);
					delay_ms(MOTORTICKGAP);
					setiolow(motorpin[motorid][1]);
					delay_ms(MOTORTICKGAP);
					setiohigh(motorpin[motorid][0]);
					delay_ms(MOTORTICKGAP);
				}
				break;
			}
			
			// A 1000,AA- 1010,A- 0010,A-B 0110,B 0100,BB- 0101,B- 0001,B-A 1001. 
			setiohigh(motorpin[motorid][0]);setiolow(motorpin[motorid][3]);
			delay_ms(MOTORTICKGAP);
			setiohigh(motorpin[motorid][2]);
			delay_ms(MOTORTICKGAP);
			setiolow(motorpin[motorid][0]);
			delay_ms(MOTORTICKGAP);
			setiohigh(motorpin[motorid][1]);
			delay_ms(MOTORTICKGAP);
			setiolow(motorpin[motorid][2]);
			delay_ms(MOTORTICKGAP);
			setiohigh(motorpin[motorid][3]);
			delay_ms(MOTORTICKGAP);
			setiolow(motorpin[motorid][1]);
			delay_ms(MOTORTICKGAP);
			setiohigh(motorpin[motorid][0]);
			delay_ms(MOTORTICKGAP);
		}
		setiolow(motorpin[motorid][0]);setiolow(motorpin[motorid][1]);setiolow(motorpin[motorid][2]);setiolow(motorpin[motorid][3]);
	}
	else
	{
		
	}

	
}

void motor42_reverse(uint16_t motorid,uint32_t step)
{
	if(motorpin[motorid][0]!=0&&motorpin[motorid][1]!=0&&motorpin[motorid][2]!=0&&motorpin[motorid][3]!=0)
	{
		setiolow(motorpin[motorid][0]);setiolow(motorpin[motorid][1]);setiolow(motorpin[motorid][2]);setiolow(motorpin[motorid][3]);
		for(int i=0;i<step;i++)
		{
			if(getiostate(motorpin[motorid][RSTOPPIN])==1)
			{
				for(int j=0;j<BACKSTEP;j++)
				{
					// A 1000,AA- 1010,A- 0010,A-B 0110,B 0100,BB- 0101,B- 0001,B-A 1001. 
					setiohigh(motorpin[motorid][0]);setiolow(motorpin[motorid][3]);
					delay_ms(MOTORTICKGAP);
					setiohigh(motorpin[motorid][2]);
					delay_ms(MOTORTICKGAP);
					setiolow(motorpin[motorid][0]);
					delay_ms(MOTORTICKGAP);
					setiohigh(motorpin[motorid][1]);
					delay_ms(MOTORTICKGAP);
					setiolow(motorpin[motorid][2]);
					delay_ms(MOTORTICKGAP);
					setiohigh(motorpin[motorid][3]);
					delay_ms(MOTORTICKGAP);
					setiolow(motorpin[motorid][1]);
					delay_ms(MOTORTICKGAP);
					setiohigh(motorpin[motorid][0]);
					delay_ms(MOTORTICKGAP);
				}
				break;
			}
			
			// A 1000,B-A 1001,B- 0001,BB- 0101,B 0100,A-B 0110,A- 0010,AA- 1010. 
			setiohigh(motorpin[motorid][0]);setiolow(motorpin[motorid][2]);
			delay_ms(MOTORTICKGAP);
			setiohigh(motorpin[motorid][3]);
			delay_ms(MOTORTICKGAP);
			setiolow(motorpin[motorid][0]);
			delay_ms(MOTORTICKGAP);
			setiohigh(motorpin[motorid][1]);
			delay_ms(MOTORTICKGAP);
			setiolow(motorpin[motorid][3]);
			delay_ms(MOTORTICKGAP);
			setiohigh(motorpin[motorid][2]);
			delay_ms(MOTORTICKGAP);
			setiolow(motorpin[motorid][1]);
			delay_ms(MOTORTICKGAP);
			setiohigh(motorpin[motorid][0]);
			delay_ms(MOTORTICKGAP);
		}
		setiolow(motorpin[motorid][0]);setiolow(motorpin[motorid][1]);setiolow(motorpin[motorid][2]);setiolow(motorpin[motorid][3]);
	}
	else
	{
		
	}

	
}


/**
  * @brief  set output io.
  * @param  argument: pin iogroup
  * @retval void
  */
void setioout(uint16_t pinnum)
{
		GPIO_TypeDef* iogroup = getiogroup(pinnum);
		GPIO_InitTypeDef GPIO_InitStruct = {0};
		
		if(iogroup==GPIOA)
		{
			__HAL_RCC_GPIOA_CLK_ENABLE();
		}
		else if(iogroup==GPIOB)
		{
			__HAL_RCC_GPIOB_CLK_ENABLE();
		}
		else if(iogroup==GPIOC)
		{
			__HAL_RCC_GPIOC_CLK_ENABLE();
		}
		else if(iogroup==GPIOD)
		{
			__HAL_RCC_GPIOD_CLK_ENABLE();
		}
		else if(iogroup==GPIOE)
		{
			__HAL_RCC_GPIOE_CLK_ENABLE();
		}
		
  GPIO_InitStruct.Pin = getiopin(pinnum);
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
  HAL_GPIO_Init(iogroup, &GPIO_InitStruct);
}

/**
  * @brief  set input io.
  * @param  argument: pin number
  * @retval void
  */
void setioin(uint16_t pinnum)
{
	GPIO_TypeDef* iogroup = getiogroup(pinnum);
  GPIO_InitTypeDef GPIO_InitStruct = {0};

		if(iogroup==GPIOA)
		{
			__HAL_RCC_GPIOA_CLK_ENABLE();
		}
		else if(iogroup==GPIOB)
		{
			__HAL_RCC_GPIOB_CLK_ENABLE();
		}
		else if(iogroup==GPIOC)
		{
			__HAL_RCC_GPIOC_CLK_ENABLE();
		}
		else if(iogroup==GPIOD)
		{
			__HAL_RCC_GPIOD_CLK_ENABLE();
		}
		else if(iogroup==GPIOE)
		{
			__HAL_RCC_GPIOE_CLK_ENABLE();
		}

  GPIO_InitStruct.Pin = getiopin(pinnum);
  GPIO_InitStruct.Mode = GPIO_MODE_INPUT;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
  HAL_GPIO_Init(iogroup, &GPIO_InitStruct);
}

/**
  * @brief  set io output high.
  * @param  argument: pin number
  * @retval void
  */
void setiohigh(uint16_t pinnum)
{
	uint16_t iopin = getiopin(pinnum);
	if(iopin==0){return;}
	HAL_GPIO_WritePin(getiogroup(pinnum), iopin,GPIO_PIN_SET);
}

/**
  * @brief  set io output low.
  * @param  argument: pin number
  * @retval void
  */
void setiolow(uint16_t pinnum)
{
	uint16_t iopin = getiopin(pinnum);
	if(iopin==0){return;}
	HAL_GPIO_WritePin(getiogroup(pinnum), iopin,GPIO_PIN_RESET);
}

/**
  * @brief  get io state.
  * @param  argument: pin number
  * @retval 0 or 1
  */
uint8_t getiostate(uint16_t pinnum)
{
	uint16_t iopin = getiopin(pinnum);
	if(iopin==0){return 0xff;}
	GPIO_PinState status = HAL_GPIO_ReadPin(getiogroup(pinnum), iopin);
	if(status==GPIO_PIN_RESET)
	{return 0;}
	else 
	{return 1;}
}

/**
  * @brief  get io group from pinnumber. stm32f4x7
  * @param  argument: pinnum
  * @retval iogroup
  */
GPIO_TypeDef* getiogroup(uint16_t pinnum)
{
	if(pinnum==23||pinnum==24||pinnum==25||pinnum==26||pinnum==29||pinnum==30||pinnum==31||pinnum==32||pinnum==67||pinnum==68||pinnum==69||pinnum==70||pinnum==71||pinnum==72||pinnum==76||pinnum==77)
	{
		return GPIOA;
	}
	else if(pinnum==35||pinnum==36||pinnum==37||pinnum==47||pinnum==48||pinnum==51||pinnum==52||pinnum==53||pinnum==54||pinnum==89||pinnum==90||pinnum==91||pinnum==92||pinnum==93||pinnum==95||pinnum==96)
	{
		return GPIOB;
	}
	else if(pinnum==7||pinnum==8||pinnum==9||pinnum==15||pinnum==16||pinnum==17||pinnum==18||pinnum==33||pinnum==34||pinnum==63||pinnum==64||pinnum==65||pinnum==66||pinnum==78||pinnum==79||pinnum==80)
	{
		return GPIOC;
	}
	else if(pinnum==55||pinnum==56||pinnum==57||pinnum==58||pinnum==59||pinnum==60||pinnum==61||pinnum==62||pinnum==81||pinnum==82||pinnum==83||pinnum==84||pinnum==85||pinnum==86||pinnum==87||pinnum==88)
	{
		return GPIOD;
	}
	else if(pinnum==1||pinnum==2||pinnum==3||pinnum==4||pinnum==5||pinnum==38||pinnum==39||pinnum==40||pinnum==41||pinnum==42||pinnum==43||pinnum==44||pinnum==45||pinnum==46||pinnum==97||pinnum==98)
	{
		return GPIOE;
	}
	else
	{
		return GPIOF;
	}

	
}

/**
  * @brief  get io pin from pinnumber. eg. PA3 is pin26 then we input 26 to get 3. stm32f4x7
  * @param  argument: pinnum
  * @retval iopin
  */
uint16_t getiopin(uint16_t pinnum)
{
	if(pinnum==16||pinnum==24||pinnum==36||pinnum==82||pinnum==98)
	{
		return GPIO_PIN_1;
	}
	else if(pinnum==1||pinnum==17||pinnum==25||pinnum==37||pinnum==83)
	{
		return GPIO_PIN_2;
	}
	else if(pinnum==2||pinnum==18||pinnum==26||pinnum==84||pinnum==89)
	{
		return GPIO_PIN_3;
	}
	else if(pinnum==3||pinnum==29||pinnum==33||pinnum==85||pinnum==90)
	{
		return GPIO_PIN_4;
	}
	else if(pinnum==4||pinnum==30||pinnum==34||pinnum==86||pinnum==91)
	{
		return GPIO_PIN_5;
	}
	else if(pinnum==5||pinnum==31||pinnum==63||pinnum==87||pinnum==92)
	{
		return GPIO_PIN_6;
	}
	else if(pinnum==32||pinnum==38||pinnum==64||pinnum==88||pinnum==93)
	{
		return GPIO_PIN_7;
	}
	else if(pinnum==39||pinnum==55||pinnum==65||pinnum==67||pinnum==95)
	{
		return GPIO_PIN_8;
	}
	else if(pinnum==40||pinnum==56||pinnum==66||pinnum==68||pinnum==96)
	{
		return GPIO_PIN_9;
	}
	else if(pinnum==41||pinnum==47||pinnum==57||pinnum==69||pinnum==78)
	{
		return GPIO_PIN_10;
	}
	else if(pinnum==42||pinnum==48||pinnum==58||pinnum==70||pinnum==79)
	{
		return GPIO_PIN_11;
	}
	else if(pinnum==43||pinnum==51||pinnum==59||pinnum==71||pinnum==80)
	{
		return GPIO_PIN_12;
	}
	else if(pinnum==7||pinnum==44||pinnum==52||pinnum==60||pinnum==72)
	{
		return GPIO_PIN_13;
	}
	else if(pinnum==8||pinnum==45||pinnum==53||pinnum==61||pinnum==76)
	{
		return GPIO_PIN_14;
	}
	else if(pinnum==9||pinnum==46||pinnum==54||pinnum==62||pinnum==77)
	{
		return GPIO_PIN_15;
	}	
	else if(pinnum==15||pinnum==35||pinnum==81)
	{
		return GPIO_PIN_0;
	}	
	else
	{
		return 0;
	}
}

