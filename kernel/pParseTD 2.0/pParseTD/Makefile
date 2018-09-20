#############################################################
# pParseTD 
#
# 2014.11.03
# Luo Lan
#
#############################################################

include ../../make.include

ifdef IS_LINUX
	SUFFIX     =
	LINK_FLAGS =	
endif

ifdef IS_WINDOWS
	SUFFIX     = .exe
	LINK_FLAGS = -Wl,--enable-auto-import -Wl,--enable-runtime-pseudo-reloc	
endif

SRC    = Solve.cpp MonoCandidates.cpp MS1Reader.cpp MS1Spectrum.cpp MS2Reader.cpp MS2Spectrum.cpp SpecDataIO.cpp StringProcessor.cpp TrainingFileReader.cpp CPeakMap.cpp Com.cpp Classification.cpp BasicFunction.cpp emass.cpp
LIBS   =
TARGET = pParseTD$(SUFFIX)

OBJS   = $(SRC:.cpp=.o)

all:	$(TARGET)

clean:
	rm -f $(OBJS) $(TARGET)

install:
	cp $(TARGET) $(INSTALL_DIR)/$(TARGET)
	
$(OBJS): $(SRC)
	$(CXX) $(CXXFLAGS) -c $(SRC)

$(TARGET):	$(OBJS)
	$(CXX) $(CXXFLAGS) $(LINK_FLAGS) -L$(BUILD_DIR) -o $(TARGET) $(OBJS) $(LIBS)
	cp $(TARGET) $(BUILD_DIR)/$(TARGET)
	