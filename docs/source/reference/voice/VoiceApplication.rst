Reference for ``VoiceApplication``
==================================

Defines how the Opus encoder will optimize the audio encoding process.

Values
------

.. attribute:: Voice

	Defines that encoding is to be optimized for voice data. This means that lower latencies are preferred to audio 
	fidelity, however quality is not completely sacrificed.

.. attribute:: Music

	Defines that encoding is to be optimized for music data. This means that higher fidelity is preferred to lower 
	latencies, which might result in interruptions on low-bandwidth connections.

.. attribute:: LowLatency

	Defines that encoding is to be optimized for low bandwidth connections. This means that lower latencies are 
	prioritied over higher fidelity, at the cost of quality.