// Petri Kannisto
// TTI/AUT
// Tampere University of Technology
// Created: 6/2017
// Modified: 7/2017

var Constants = require("./Constants");

/**
 * Holds the power equipment (pumps) in the process.
 */
function PowerEquipmentManager()
{
	var PUMP_NOMINAL_FLOW_l_min = 4;
	var MAX_PRESSURE_DIFF_atm = 0.3;
	
	var self = this;
	
	// False means off
	var m_eqStates = {};
	m_eqStates[Constants.PUMP_1] = false;
	m_eqStates[Constants.PUMP_2] = false;
	m_eqStates[Constants.PUMPS_PRESET] = false;
	
	
	/**
	 * Returns the state of a piece of equipment.
	 */
	this.getEquipmentState = function(name)
	{
		if (!m_eqStates.hasOwnProperty(name))
		{
			self.private_logAndReturnError("There is no equipment called \"" + name + "\"");
		}
		
		return m_eqStates[name];
	};
	
	/**
	 * Sets the state of a piece of equipment.
	 */
	this.setEquipmentState = function(name, value)
	{
		if (!m_eqStates.hasOwnProperty(name))
		{
			self.private_logAndReturnError("There is no equipment called \"" + name + "\"");
		}
		
		if (typeof(value) !== "boolean")
		{
			self.private_logAndReturnError("Equipment state must be of type boolean");
		}
		
		m_eqStates[name] = value;
	};
	
	/**
	 * Calculates the flow generated by a pump (in l/min).
	 */
	this.calculateFlow_l_min = function(name, yForThrottling)
	{
		if (!m_eqStates.hasOwnProperty(name))
		{
			self.private_logAndReturnError("There is no equipment called \"" + name + "\"");
		}
		
		// Pumps preset enabled?
		if (!m_eqStates[Constants.PUMPS_PRESET])
		{
			return 0;
		}
		
		if (typeof(yForThrottling) !== "number")
		{
			self.private_logAndReturnError("Throttling constant must be of type number");
		}
		
		var pumpState = m_eqStates[name];
		
		if (pumpState === true)
		{
			//	         Qn
			// Q = ------------
			//      Qn * y
			//      ------ + 1
			//        pm
			return PUMP_NOMINAL_FLOW_l_min / (PUMP_NOMINAL_FLOW_l_min * yForThrottling / MAX_PRESSURE_DIFF_atm + 1);	
		}
		else
		{
			// The pump is off
			return 0;
		}
	};
	
	this.private_logAndReturnError = function(err)
	{
		console.error(err);
		throw err;
	};
}

module.exports = PowerEquipmentManager;