import matplotlib.pyplot as plt
from numpy import arange, array, nditer

TPS = 60
simT = 6
setAccel = 1
setSpeed = 5
startPos = 0
startSpeed = 0.000001

Ts = range(simT*TPS)
Vs = [-1]*len(Ts)
Ps = [-1]*len(Ts)

targetPos = 0
curPos = startPos
curSpeed = startSpeed

for i in Ts:

    if i == TPS:
        targetPos = 5

    distanceToTarget = max(curPos, targetPos) - min(curPos, targetPos)
    timeToTarget = distanceToTarget / setSpeed
    direction = 1 if curPos < targetPos else -1
    signedSetSpeed = direction * setSpeed

    timeToDecelerate = curSpeed / setAccel
    
    if (timeToTarget <= (1/TPS)):
        curSpeed = 0.000001
    else:
        curSpeed = signedSetSpeed
        #acceldirection = 1 if timeToTarget > timeToDecelerate else -1
        #signedSetAccel = acceldirection * setAccel
        #curSpeed += direction * (signedSetAccel / TPS)

    #sim
    curPos += (1/TPS)*curSpeed
    Ps[i] = curPos
    Vs[i] = curSpeed

plt.plot(Ts, Vs, label="V")
plt.plot(Ts, Ps, label="P")
plt.legend()
plt.show()
