#############################################################
# pTop 
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

SRC    = main.cpp BasicTools.cpp Configuration.cpp CreateIndex.cpp MainFlow.cpp MapAAMass.cpp PrePTMForm.cpp ResFilter.cpp SearchEngine.cpp TagFlow.cpp TAGPriorityQueue.cpp TagSearcher.cpp
LIBS   =
TARGET = pTop$(SUFFIX)

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
	