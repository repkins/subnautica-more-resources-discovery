# More Resources Discovery for Subnautica

Gameplay improving modification on resource scanning for Subnautica game running on Unity Engine.

## Overview

This modification adds background running task which ensures expected behaviour of scanning resources gameplay mechanic, using **combination of Unity coroutines and I/O thread**:
 - I/O thread provides loading areas (in binary form) from save storage;
 - Coroutines faciliates loading areas outside of world streaming range around player position from disk and deserialization into game objects under configured rate.

Rate resembles how much mod can focus on deserialization to game objects on a single frame, at the cost of decreased FPS.
